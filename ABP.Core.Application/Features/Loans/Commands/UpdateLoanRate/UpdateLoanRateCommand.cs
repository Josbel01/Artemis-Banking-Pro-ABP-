using MediatR;

namespace ABP.Core.Application.Features.Loans.Commands.UpdateLoanRate
{
    public class UpdateLoanRateCommand : IRequest<bool>
    {
        public string Id { get; set; } = string.Empty;
        public decimal AnnualInterestRate { get; set; }
    }
}
