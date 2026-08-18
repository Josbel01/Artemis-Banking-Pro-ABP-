using ABP.Core.Application.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Loans.Commands.UpdateLoanRate
{
    public class UpdateLoanRateCommandHandler : IRequestHandler<UpdateLoanRateCommand, bool>
    {
        private readonly ILoanService _loanService;

        public UpdateLoanRateCommandHandler(ILoanService loanService)
        {
            _loanService = loanService;
        }

        public async Task<bool> Handle(UpdateLoanRateCommand request, CancellationToken cancellationToken)
        {
            if (!int.TryParse(request.Id, out int id)) return false;

            var loan = await _loanService.GetByIdAsync(id);
            if (loan == null) return false;

            loan.AnnualInterestRate = request.AnnualInterestRate;
            await _loanService.UpdateAsync(loan, id);

            return true;
        }
    }
}

