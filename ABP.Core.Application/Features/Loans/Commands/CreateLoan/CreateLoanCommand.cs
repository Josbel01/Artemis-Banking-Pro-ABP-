using MediatR;

namespace ABP.Core.Application.Features.Loans.Commands.CreateLoan
{
    public class CreateLoanCommand : IRequest<object>
    {
        public string ClientId { get; set; } = string.Empty;
        public decimal CapitalAmount { get; set; }
        public int TermInMonths { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public bool ConfirmHighRisk { get; set; }
    }
}
