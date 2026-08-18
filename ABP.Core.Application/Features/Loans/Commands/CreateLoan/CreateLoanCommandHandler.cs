using ABP.Core.Application.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Application.Exceptions;
using ABP.Core.Application.Helpers;
using ABP.Core.Application.Dtos.Loans;
using ABP.Core.Application.Dtos.Transactions;
using System.Linq;
using System.Transactions;

namespace ABP.Core.Application.Features.Loans.Commands.CreateLoan
{
    public class CreateLoanCommandHandler : IRequestHandler<CreateLoanCommand, object>
    {
        private readonly ILoanService _loanService;
        private readonly ILoanInstallmentService _loanInstallmentService;
        private readonly ISavingAccountService _savingAccountService;
        private readonly ITransactionService _transactionService;

        public CreateLoanCommandHandler(ILoanService loanService, ILoanInstallmentService loanInstallmentService,
            ISavingAccountService savingAccountService, ITransactionService transactionService)
        {
            _loanService = loanService;
            _loanInstallmentService = loanInstallmentService;
            _savingAccountService = savingAccountService;
            _transactionService = transactionService;
        }

        public async Task<object> Handle(CreateLoanCommand request, CancellationToken cancellationToken)
        {
            if (!new[] { 6, 12, 18, 24, 30, 36, 42, 48, 54, 60 }.Contains(request.TermInMonths))
                throw new ApiException("El plazo debe estar entre las opciones permitidas de 6 a 60 meses.");

            var clientLoans = await _loanService.GetAllByClientIdAsync(request.ClientId);
            if (clientLoans.Any(loan => loan.Status == LoanStatus.Active))
                throw new ApiException("El cliente ya posee un préstamo activo.");

            var principalAccount = (await _savingAccountService.GetAllByClientIdAsync(request.ClientId))
                .FirstOrDefault(account => account.AccountType == SavingAccountType.Main && account.Status == SavingAccountStatus.Active);
            if (principalAccount == null)
                throw new ApiException("El cliente no tiene una cuenta principal activa.");

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var loan = new LoanDto
            {
                Id = 0,
                ClientId = request.ClientId,
                AmountApproved = request.CapitalAmount,
                TermInMonths = request.TermInMonths,
                AnnualInterestRate = request.AnnualInterestRate,
                Status = LoanStatus.Active,
                LoanNumber = new Random().Next(100000000, 999999999).ToString(),
                TotalInstallments = request.TermInMonths,
                AmountPending = request.CapitalAmount,
                ClientPaymentStatus = "Al día"
            };

            var created = await _loanService.AddAsync(loan);
            var installments = LoanAmortizationCalculator.GenerateAmortizationSchedule(
                created.AmountApproved, created.AnnualInterestRate, created.TermInMonths, DateTime.UtcNow);
            foreach (var installment in installments)
            {
                installment.LoanId = created.Id;
                await _loanInstallmentService.AddAsync(new LoanInstallmentDto
                {
                    Id = 0,
                    LoanId = installment.LoanId,
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

            principalAccount.Balance += created.AmountApproved;
            await _savingAccountService.UpdateAsync(principalAccount, principalAccount.Id);
            await _transactionService.AddAsync(new TransactionDto
            {
                SavingAccountId = principalAccount.Id,
                Amount = created.AmountApproved,
                Type = TransactionType.Credit,
                TransactionDate = DateTime.UtcNow,
                Origin = created.LoanNumber,
                Beneficiary = principalAccount.AccountNumber,
                Status = ABP.Core.Domain.Common.Enums.TransactionStatus.Approved
            });
            scope.Complete();

            return new
            {
                id = created.Id.ToString(),
                loanNumber = created.LoanNumber,
                clientId = created.ClientId,
                capitalAmount = created.AmountApproved,
                termInMonths = created.TermInMonths,
                annualInterestRate = created.AnnualInterestRate,
                monthlyInstallment = LoanAmortizationCalculator.CalculateMonthlyPayment(created.AmountApproved, created.AnnualInterestRate, created.TermInMonths),
                totalAmountToPay = installments.Sum(i => i.InstallmentAmount),
                status = "Activo"
            };
        }
    }
}
