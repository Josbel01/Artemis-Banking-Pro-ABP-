using ABP.Core.Application.Dtos.Transactions;
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
    public class TransactionService : GenericService<Transaction, TransactionDto>, ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ISavingAccountRepository _savingAccountRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ICardTransactionRepository _cardTransactionRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly ILoanInstallmentRepository _loanInstallmentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            ITransactionRepository transactionRepository,
            ISavingAccountRepository savingAccountRepository,
            ICreditCardRepository creditCardRepository,
            ICardTransactionRepository cardTransactionRepository,
            ILoanRepository loanRepository,
            ILoanInstallmentRepository loanInstallmentRepository,
            IMapper mapper,
            ILoggerFactory loggerFactory) : base(transactionRepository, mapper, loggerFactory.CreateLogger<GenericService<Transaction, TransactionDto>>())
        {
            _transactionRepository = transactionRepository;
            _savingAccountRepository = savingAccountRepository;
            _creditCardRepository = creditCardRepository;
            _cardTransactionRepository = cardTransactionRepository;
            _loanRepository = loanRepository;
            _loanInstallmentRepository = loanInstallmentRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<TransactionService>();
        }

        private List<string> _includes => new() { "SavingAccount" };

        public override async Task<TransactionDto?> GetByIdAsync(int id)
        {
            var list = await _transactionRepository.GetAllListWithInclude(_includes);
            var entity = list.FirstOrDefault(t => t.Id == id);
            return entity == null ? null : _mapper.Map<TransactionDto>(entity);
        }

        public override async Task<List<TransactionDto>> GetAllAsync()
        {
            var list = await _transactionRepository.GetAllListWithInclude(_includes);
            return _mapper.Map<List<TransactionDto>>(list);
        }

        public async Task<List<TransactionDto>> GetTransactionsByAccountIdAsync(int accountId)
        {
            var allTransactions = await _transactionRepository.GetAllListWithInclude(_includes);
            var accountTransactions = allTransactions
                .Where(t => t.SavingAccountId == accountId)
                .OrderByDescending(t => t.TransactionDate)
                .ToList();

            return _mapper.Map<List<TransactionDto>>(accountTransactions);
        }

        public async Task<bool> TransferAsync(SaveTransferDto dto)
        {
            _logger.LogInformation("Initiating transfer of RD${Amount} from account {Origin} to account {Destination}", dto.Amount, dto.OriginAccountNumber, dto.DestinationAccountNumber);
            var allAccounts = await _savingAccountRepository.GetAllListAsync();
            var originAccount = allAccounts.FirstOrDefault(a => a.AccountNumber == dto.OriginAccountNumber);
            var destinationAccount = allAccounts.FirstOrDefault(a => a.AccountNumber == dto.DestinationAccountNumber);

            if (originAccount == null || destinationAccount == null)
            {
                _logger.LogWarning("Transfer failed: Origin or destination account not found.");
                return false;
            }

            if (originAccount.Balance < dto.Amount)
            {
                _logger.LogWarning("Transfer failed: Insufficient funds in origin account {Origin}", dto.OriginAccountNumber);
                return false;
            }

            originAccount.Balance -= dto.Amount;
            await _savingAccountRepository.UpdateAsync(originAccount.Id, originAccount);

            destinationAccount.Balance += dto.Amount;
            await _savingAccountRepository.UpdateAsync(destinationAccount.Id, destinationAccount);

            var debitTransaction = new Transaction
            {
                SavingAccountId = originAccount.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = dto.Amount,
                Type = TransactionType.Debit,
                Beneficiary = destinationAccount.AccountNumber,
                Origin = originAccount.AccountNumber,
                Status = TransactionStatus.Approved
            };
            await _transactionRepository.AddAsync(debitTransaction);

            var creditTransaction = new Transaction
            {
                SavingAccountId = destinationAccount.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = dto.Amount,
                Type = TransactionType.Credit,
                Beneficiary = destinationAccount.AccountNumber,
                Origin = originAccount.AccountNumber,
                Status = TransactionStatus.Approved
            };
            await _transactionRepository.AddAsync(creditTransaction);

            _logger.LogInformation("Transfer completed successfully.");
            return true;
        }

        public async Task<bool> CashAdvanceAsync(SaveCashAdvanceDto dto)
        {
            _logger.LogInformation("Initiating cash advance of RD${Amount} from credit card {Origin} to account {Destination}", dto.Amount, dto.OriginCreditCardNumber, dto.DestinationAccountNumber);
            var allCreditCards = await _creditCardRepository.GetAllListAsync();
            var creditCard = allCreditCards.FirstOrDefault(c => c.CardNumber == dto.OriginCreditCardNumber);

            var allAccounts = await _savingAccountRepository.GetAllListAsync();
            var destinationAccount = allAccounts.FirstOrDefault(a => a.AccountNumber == dto.DestinationAccountNumber);

            if (creditCard == null || destinationAccount == null)
            {
                _logger.LogWarning("Cash advance failed: Origin credit card or destination account not found.");
                return false;
            }

            var availableLimit = creditCard.CreditLimit - creditCard.CurrentDebt;
            var amountWithInterest = dto.Amount + (dto.Amount * 0.0625m); // 6.25% de interes

            if (availableLimit < amountWithInterest)
            {
                _logger.LogWarning("Cash advance failed: Insufficient credit limit in credit card {Origin}", dto.OriginCreditCardNumber);
                return false; 
            }

            creditCard.CurrentDebt += amountWithInterest;
            await _creditCardRepository.UpdateAsync(creditCard.Id, creditCard);

            destinationAccount.Balance += dto.Amount;
            await _savingAccountRepository.UpdateAsync(destinationAccount.Id, destinationAccount);

            var cardTransaction = new CardTransaction
            {
                CreditCardId = creditCard.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = amountWithInterest, 
                CommerceName = "Avance de Efectivo",
                Status = TransactionStatus.Approved
            };
            await _cardTransactionRepository.AddAsync(cardTransaction);

            var creditTransaction = new Transaction
            {
                SavingAccountId = destinationAccount.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = dto.Amount,
                Type = TransactionType.CashAdvance,
                Beneficiary = destinationAccount.AccountNumber,
                Origin = creditCard.CardNumber.Substring(creditCard.CardNumber.Length - 4),
                Status = TransactionStatus.Approved
            };
            await _transactionRepository.AddAsync(creditTransaction);

            _logger.LogInformation("Cash advance completed successfully.");
            return true;
        }

        public async Task<bool> CreditCardPaymentAsync(SaveCreditCardPaymentDto dto)
        {
            _logger.LogInformation("Initiating credit card payment of RD${Amount} to card {Card}", dto.Amount, dto.CreditCardNumber);
            var allCreditCards = await _creditCardRepository.GetAllListAsync();
            var creditCard = allCreditCards.FirstOrDefault(c => c.CardNumber == dto.CreditCardNumber);

            var allAccounts = await _savingAccountRepository.GetAllListAsync();
            var originAccount = allAccounts.FirstOrDefault(a => a.AccountNumber == dto.OriginAccountNumber);

            if (creditCard == null || originAccount == null)
            {
                _logger.LogWarning("Credit card payment failed: Card or account not found.");
                return false;
            }

            if (creditCard.Status != CreditCardStatus.Active)
            {
                _logger.LogWarning("Credit card payment failed: Card is inactive.");
                return false;
            }

            if (originAccount.Status != SavingAccountStatus.Active)
            {
                _logger.LogWarning("Credit card payment failed: Origin account is inactive.");
                return false;
            }

            if (creditCard.CurrentDebt <= 0)
            {
                _logger.LogWarning("Credit card payment failed: Card has no debt.");
                return false;
            }

            // Cap payment to actual debt (no sobrepago)
            var effectiveAmount = Math.Min(dto.Amount, creditCard.CurrentDebt);

            if (originAccount.Balance < effectiveAmount)
            {
                _logger.LogWarning("Credit card payment failed: Insufficient funds in origin account.");
                return false;
            }

            originAccount.Balance -= effectiveAmount;
            await _savingAccountRepository.UpdateAsync(originAccount.Id, originAccount);

            creditCard.CurrentDebt -= effectiveAmount;
            await _creditCardRepository.UpdateAsync(creditCard.Id, creditCard);

            // Register card transaction
            var cardTransaction = new CardTransaction
            {
                CreditCardId = creditCard.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = effectiveAmount,
                CommerceName = "Pago en Cuenta",
                Status = TransactionStatus.Approved
            };
            await _cardTransactionRepository.AddAsync(cardTransaction);

            // Register debit in savings account
            var debitTransaction = new Transaction
            {
                SavingAccountId = originAccount.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = effectiveAmount,
                Type = TransactionType.Debit,
                Beneficiary = creditCard.CardNumber.Substring(creditCard.CardNumber.Length - 4),
                Origin = originAccount.AccountNumber,
                Status = TransactionStatus.Approved
            };
            await _transactionRepository.AddAsync(debitTransaction);

            _logger.LogInformation("Credit card payment completed successfully.");
            return true;
        }

        public async Task<bool> LoanPaymentAsync(SaveLoanPaymentDto dto)
        {
            _logger.LogInformation("Initiating loan payment of RD${Amount} to loan {Loan}", dto.Amount, dto.LoanNumber);
            var allLoans = await _loanRepository.GetAllListAsync();
            var loan = allLoans.FirstOrDefault(l => l.LoanNumber == dto.LoanNumber);

            var allAccounts = await _savingAccountRepository.GetAllListAsync();
            var originAccount = allAccounts.FirstOrDefault(a => a.AccountNumber == dto.OriginAccountNumber);

            if (loan == null || originAccount == null)
            {
                _logger.LogWarning("Loan payment failed: Loan or account not found.");
                return false;
            }

            if (loan.Status != LoanStatus.Active)
            {
                _logger.LogWarning("Loan payment failed: Loan is not active.");
                return false;
            }

            if (originAccount.Status != SavingAccountStatus.Active)
            {
                _logger.LogWarning("Loan payment failed: Origin account is inactive.");
                return false;
            }

            if (loan.AmountPending <= 0)
            {
                _logger.LogWarning("Loan payment failed: Loan has no pending amount.");
                return false;
            }

            // Cap payment to actual pending amount
            var effectiveAmount = Math.Min(dto.Amount, loan.AmountPending);

            if (originAccount.Balance < effectiveAmount)
            {
                _logger.LogWarning("Loan payment failed: Insufficient funds in origin account.");
                return false;
            }

            originAccount.Balance -= effectiveAmount;
            await _savingAccountRepository.UpdateAsync(originAccount.Id, originAccount);

            loan.AmountPending -= effectiveAmount;
            loan.PaidInstallments++;
            if (loan.AmountPending <= 0)
            {
                loan.AmountPending = 0;
                loan.Status = LoanStatus.Completed;
            }
            await _loanRepository.UpdateAsync(loan.Id, loan);

            // Apply to oldest pending installment first
            var installments = await _loanInstallmentRepository.GetAllListAsync();
            var pendingInstallments = installments
                .Where(i => i.LoanId == loan.Id && i.PaymentStatus != PaymentStatus.Paid)
                .OrderBy(i => i.DueDate)
                .ToList();

            decimal remaining = effectiveAmount;
            foreach (var inst in pendingInstallments)
            {
                if (remaining <= 0) break;
                var payment = Math.Min(remaining, inst.PendingAmount);
                inst.PendingAmount -= payment;
                remaining -= payment;
                if (inst.PendingAmount <= 0)
                {
                    inst.PaymentStatus = PaymentStatus.Paid;
                    inst.IsLate = false;
                }
                else
                {
                    inst.PaymentStatus = PaymentStatus.PartiallyPaid;
                }
                await _loanInstallmentRepository.UpdateAsync(inst.Id, inst);
            }

            // Register debit in savings account
            var debitTransaction = new Transaction
            {
                SavingAccountId = originAccount.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = effectiveAmount,
                Type = TransactionType.LoanPayment,
                Beneficiary = loan.LoanNumber,
                Origin = originAccount.AccountNumber,
                Status = TransactionStatus.Approved
            };
            await _transactionRepository.AddAsync(debitTransaction);

            _logger.LogInformation("Loan payment completed successfully.");
            return true;
        }
    }
}
