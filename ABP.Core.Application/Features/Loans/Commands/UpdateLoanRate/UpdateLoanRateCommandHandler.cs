using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Helpers;
using ABP.Core.Domain.Common.Enums;
using System.Linq;

namespace ABP.Core.Application.Features.Loans.Commands.UpdateLoanRate
{
    public class UpdateLoanRateCommandHandler : IRequestHandler<UpdateLoanRateCommand, bool>
    {
        private readonly ILoanService _loanService;
        private readonly ILoanInstallmentService _loanInstallmentService;

        public UpdateLoanRateCommandHandler(ILoanService loanService, ILoanInstallmentService loanInstallmentService)
        {
            _loanService = loanService;
            _loanInstallmentService = loanInstallmentService;
        }

        public async Task<bool> Handle(UpdateLoanRateCommand request, CancellationToken cancellationToken)
        {
            if (!int.TryParse(request.Id, out int id)) return false;

            var loan = await _loanService.GetByIdAsync(id);
            if (loan == null) return false;

            var pendingInstallments = (await _loanInstallmentService.GetAllByLoanIdAsync(id))
                .Where(installment => installment.PaymentStatus == PaymentStatus.Pending && installment.PendingAmount > 0)
                .OrderBy(installment => installment.InstallmentNumber)
                .ToList();

            if (pendingInstallments.Count > 0)
            {
                var remainingPrincipal = pendingInstallments.Sum(installment => installment.CapitalAmount);
                var recalculated = LoanAmortizationCalculator.GenerateAmortizationSchedule(
                    remainingPrincipal, request.AnnualInterestRate, pendingInstallments.Count, DateTime.UtcNow);
                for (var index = 0; index < pendingInstallments.Count; index++)
                {
                    var current = pendingInstallments[index];
                    var updated = recalculated[index];
                    current.InstallmentAmount = updated.InstallmentAmount;
                    current.InterestAmount = updated.InterestAmount;
                    current.CapitalAmount = updated.CapitalAmount;
                    current.PendingAmount = updated.PendingAmount;
                    await _loanInstallmentService.UpdateAsync(current, current.Id);
                }
                loan.AmountPending = pendingInstallments.Sum(installment => installment.PendingAmount);
            }

            loan.AnnualInterestRate = request.AnnualInterestRate;
            await _loanService.UpdateAsync(loan, id);

            return true;
        }
    }
}

