using ABP.Core.Application.Dtos.CreditCards;
using ABP.Core.Application.Dtos.Email;
using ABP.Core.Application.Dtos.Loans;
using ABP.Core.Application.Dtos.Transactions;
using ABP.Core.Application.Helpers;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABP.Core.Application.Services
{
    public class LoanService : GenericService<Loan, LoanDto>, ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ILoanInstallmentService _loanInstallmentService;
        private readonly ISavingAccountService _savingAccountService;
        private readonly ITransactionService _transactionService;
        private readonly IEmailService _emailService;
        private readonly ICreditCardService _creditCardService;
        private readonly IBaseAccountService _accountService;
        private readonly IMapper _mapper;
        private readonly ILogger<LoanService> _logger;

        public LoanService(
            ILoanRepository loanRepository,
            ILoanInstallmentService loanInstallmentService,
            ISavingAccountService savingAccountService,
            ITransactionService transactionService,
            IEmailService emailService,
            ICreditCardService creditCardService,
            IBaseAccountService accountService,
            IMapper mapper, 
            ILoggerFactory loggerFactory) 
            : base(loanRepository, mapper, loggerFactory.CreateLogger<GenericService<Loan, LoanDto>>())
        {
            _loanRepository = loanRepository;
            _loanInstallmentService = loanInstallmentService;
            _savingAccountService = savingAccountService;
            _transactionService = transactionService;
            _emailService = emailService;
            _creditCardService = creditCardService;
            _accountService = accountService;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<LoanService>();
        }

        public override async Task<List<LoanDto>> GetAllAsync()
        {
            var loans = await base.GetAllAsync();
            FixInstallmentCounts(loans);
            return loans;
        }

        public async Task<List<LoanDto>> GetAllByClientIdAsync(string clientId)
        {
            _logger.LogInformation("Retrieving all loans for client ID: {ClientId}", clientId);
            var loans = await _loanRepository.GetAllListAsync();
            var clientLoans = loans.Where(l => l.ClientId == clientId).ToList();
            _logger.LogInformation("Found {Count} loans for client ID: {ClientId}", clientLoans.Count, clientId);
            var dtos = _mapper.Map<List<LoanDto>>(clientLoans);
            FixInstallmentCounts(dtos);
            return dtos;
        }

        public async Task<LoanDto?> GetByLoanNumberAsync(string loanNumber)
        {
            _logger.LogInformation("Retrieving loan by loan number");
            var loans = await _loanRepository.GetAllListAsync();
            var loan = loans.FirstOrDefault(l => l.LoanNumber == loanNumber);
            
            if (loan == null)
            {
                _logger.LogWarning("Loan not found");
                return null;
            }

            _logger.LogInformation("Loan found");
            return _mapper.Map<LoanDto>(loan);
        }

        public decimal CalculateClientDebt(string clientId, List<LoanDto> loans, List<CreditCardDto> creditCards)
        {
            decimal loanDebt = loans
                .Where(l => l.ClientId == clientId && l.Status == LoanStatus.Active)
                .Sum(l => l.AmountPending);
            decimal cardDebt = creditCards
                .Where(c => c.ClientId == clientId && c.Status == CreditCardStatus.Active)
                .Sum(c => c.CurrentDebt);
            return loanDebt + cardDebt;
        }

        public async Task<(bool hasRisk, string message, decimal avgDebt, decimal currentDebt, decimal projectedDebt)> EvaluateRiskAsync(string clientId, decimal principal, decimal rate, int term)
        {
            var allLoans = await GetAllAsync();
            var allCards = await _creditCardService.GetAllAsync();
            var allUsers = await GetAllUsersAsync();
            var clientUsers = allUsers.Where(u => u.Roles != null && u.Roles.Contains("Client")).ToList();

            decimal totalSystemDebt = 0;
            foreach (var cu in clientUsers)
            {
                decimal loanDebt = allLoans.Where(l => l.ClientId == cu.Id && l.Status == LoanStatus.Active).Sum(l => l.AmountPending);
                decimal cardDebt = allCards.Where(c => c.ClientId == cu.Id && c.Status == CreditCardStatus.Active).Sum(c => c.CurrentDebt);
                totalSystemDebt += loanDebt + cardDebt;
            }
            decimal avgDebt = clientUsers.Count > 0 ? totalSystemDebt / clientUsers.Count : 0;

            decimal clientCurrentLoanDebt = allLoans.Where(l => l.ClientId == clientId && l.Status == LoanStatus.Active).Sum(l => l.AmountPending);
            decimal clientCurrentCardDebt = allCards.Where(c => c.ClientId == clientId && c.Status == CreditCardStatus.Active).Sum(c => c.CurrentDebt);
            decimal clientCurrentDebt = clientCurrentLoanDebt + clientCurrentCardDebt;

            var tempInstallments = LoanAmortizationCalculator.GenerateAmortizationSchedule(principal, rate, term, DateTime.Now);
            decimal newLoanTotal = tempInstallments.Sum(i => i.InstallmentAmount);
            decimal projectedDebt = clientCurrentDebt + newLoanTotal;

            string riskMessage = null;
            if (clientCurrentDebt > avgDebt)
                riskMessage = "Este cliente se considera de alto riesgo, ya que su deuda actual supera el promedio del sistema.";
            else if (projectedDebt > avgDebt)
                riskMessage = "Asignar este pr\u00e9stamo convertir\u00e1 al cliente en un cliente de alto riesgo, ya que su deuda superar\u00e1 el umbral promedio del sistema.";

            return (riskMessage != null, riskMessage ?? "", avgDebt, clientCurrentDebt, projectedDebt);
        }

        public async Task<LoanDto?> ProcessLoanCreationAsync(string clientId, decimal principal, decimal rate, int term, string assignedByUserId)
        {
            var rnd = new Random();
            string loanNumber = rnd.Next(100000000, 999999999).ToString();

            var installments = LoanAmortizationCalculator.GenerateAmortizationSchedule(principal, rate, term, DateTime.Now);
            decimal totalPending = installments.Sum(i => i.InstallmentAmount);

            var dto = new LoanDto
            {
                Id = 0,
                ClientId = clientId,
                LoanNumber = loanNumber,
                AmountApproved = principal,
                AmountPending = totalPending,
                AnnualInterestRate = rate,
                TermInMonths = term,
                Status = LoanStatus.Active,
                TotalInstallments = term,
                AssignedByUserId = assignedByUserId
            };

            var createdLoan = await AddAsync(dto);
            if (createdLoan == null) return null;

            foreach (var installment in installments)
            {
                await _loanInstallmentService.AddAsync(new LoanInstallmentDto
                {
                    Id = 0,
                    LoanId = createdLoan.Id,
                    InstallmentNumber = installment.InstallmentNumber,
                    DueDate = installment.DueDate,
                    InstallmentAmount = installment.InstallmentAmount,
                    InterestAmount = installment.InterestAmount,
                    CapitalAmount = installment.CapitalAmount,
                    PendingAmount = installment.PendingAmount,
                    PaymentStatus = installment.PaymentStatus,
                    IsLate = installment.IsLate
                });
            }

            // Deposit to main account
            var clientAccounts = await _savingAccountService.GetAllByClientIdAsync(clientId);
            var mainAccount = clientAccounts.FirstOrDefault(a => a.AccountType == SavingAccountType.Main && a.Status == SavingAccountStatus.Active);
            if (mainAccount != null)
            {
                mainAccount.Balance += principal;
                await _savingAccountService.UpdateAsync(mainAccount, mainAccount.Id);
                await _transactionService.AddAsync(new TransactionDto
                {
                    SavingAccountId = mainAccount.Id,
                    Amount = principal,
                    Type = TransactionType.Credit,
                    TransactionDate = DateTime.Now,
                    Origin = loanNumber,
                    Beneficiary = mainAccount.AccountNumber,
                    Status = TransactionStatus.Approved
                });
            }

            return createdLoan;
        }

        private async Task<List<Dtos.User.UserDto>> GetAllUsersAsync()
        {
            return await _accountService.GetAllUser();
        }

        private void FixInstallmentCounts(List<LoanDto> loans)
        {
            foreach (var loan in loans)
            {
                if (loan.TotalInstallments == 0 && loan.TermInMonths > 0)
                    loan.TotalInstallments = loan.TermInMonths;

                // Calculate real PaidInstallments from installments
                if (loan.LoanInstallments != null && loan.LoanInstallments.Any())
                {
                    loan.PaidInstallments = loan.LoanInstallments.Count(i => i.PaymentStatus == Domain.Common.Enums.PaymentStatus.Paid);
                }
            }
        }
    }
}
