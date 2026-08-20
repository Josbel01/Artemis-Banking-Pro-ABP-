using ABP.Core.Application.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ABP.Core.Domain.Common.Enums;

namespace ABP.Core.Application.Features.Loans.Queries.GetLoanById
{
    public class GetLoanByIdQueryHandler : IRequestHandler<GetLoanByIdQuery, object?>
    {
        private readonly ILoanService _loanService;
        private readonly ILoanInstallmentService _installmentService;
        private readonly IBaseAccountService _accountService;

        public GetLoanByIdQueryHandler(ILoanService loanService, ILoanInstallmentService installmentService, IBaseAccountService accountService)
        {
            _loanService = loanService;
            _installmentService = installmentService;
            _accountService = accountService;
        }

        public async Task<object?> Handle(GetLoanByIdQuery request, CancellationToken cancellationToken)
        {
            if (!int.TryParse(request.Id, out int id)) return null;

            var loan = await _loanService.GetByIdAsync(id);
            if (loan == null) return null;

            var installments = await _installmentService.GetAllAsync();
            var loanInstallments = installments.Where(i => i.LoanId == id).OrderBy(i => i.InstallmentNumber).ToList();

            var client = await _accountService.GetUserById(loan.ClientId);
            var clientFullName = client != null ? $"{client.FirstName} {client.LastName}" : "";

            // Determine payment status based on late installments
            var hasLate = loanInstallments.Any(i => i.IsLate);
            var allPaid = loanInstallments.All(i => i.PaymentStatus == PaymentStatus.Paid);
            var clientPaymentStatus = hasLate ? "Atrasado" : (allPaid ? "Al día" : "Al día");

            var paidCount = loanInstallments.Count(i => i.PaymentStatus == PaymentStatus.Paid);

            return new
            {
                id = loan.Id.ToString(),
                loanNumber = loan.LoanNumber,
                clientId = loan.ClientId,
                clientFullName = clientFullName,
                capitalAmount = loan.AmountApproved,
                annualInterestRate = loan.AnnualInterestRate,
                termInMonths = loan.TermInMonths,
                monthlyInstallment = loanInstallments.FirstOrDefault()?.InstallmentAmount ?? 0,
                pendingAmount = loan.AmountPending,
                totalInstallments = loanInstallments.Count,
                paidInstallments = paidCount,
                status = loan.Status,
                clientPaymentStatus = clientPaymentStatus,
                createdAt = System.DateTime.Now,
                amortization = loanInstallments.Select(i => new
                {
                    installmentNumber = i.InstallmentNumber,
                    dueDate = i.DueDate,
                    installmentAmount = i.InstallmentAmount,
                    interestAmount = i.InterestAmount,
                    capitalAmount = i.CapitalAmount,
                    pendingAmount = i.PendingAmount,
                    paymentStatus = i.PaymentStatus.ToString(),
                    isLate = i.IsLate,
                    paidDate = i.PaidDate
                }).ToList()
            };
        }
    }
}
