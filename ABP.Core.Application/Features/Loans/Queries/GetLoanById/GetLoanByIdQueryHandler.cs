using ABP.Core.Application.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace ABP.Core.Application.Features.Loans.Queries.GetLoanById
{
    public class GetLoanByIdQueryHandler : IRequestHandler<GetLoanByIdQuery, object?>
    {
        private readonly ILoanService _loanService;
        private readonly ILoanInstallmentService _installmentService;

        public GetLoanByIdQueryHandler(ILoanService loanService, ILoanInstallmentService installmentService)
        {
            _loanService = loanService;
            _installmentService = installmentService;
        }

        public async Task<object?> Handle(GetLoanByIdQuery request, CancellationToken cancellationToken)
        {
            if (!int.TryParse(request.Id, out int id)) return null;

            var loan = await _loanService.GetByIdAsync(id);
            if (loan == null) return null;

            var installments = await _installmentService.GetAllAsync();
            var loanInstallments = installments.Where(i => i.LoanId == id).OrderBy(i => i.InstallmentNumber).ToList();

            return new
            {
                id = loan.Id.ToString(),
                loanNumber = loan.LoanNumber,
                clientId = loan.ClientId,
                clientFullName = "", 
                capitalAmount = loan.AmountApproved,
                annualInterestRate = loan.AnnualInterestRate,
                termInMonths = loan.TermInMonths,
                monthlyInstallment = loanInstallments.FirstOrDefault()?.InstallmentAmount ?? 0,
                pendingAmount = loan.AmountPending,
                status = loan.Status,
                clientPaymentStatus = "Al día",
                createdAt = System.DateTime.Now,
                amortization = new object[0]
            };
        }
    }
}



