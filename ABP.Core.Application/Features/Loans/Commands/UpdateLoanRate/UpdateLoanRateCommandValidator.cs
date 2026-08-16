using FluentValidation;

namespace ABP.Core.Application.Features.Loans.Commands.UpdateLoanRate
{
    public class UpdateLoanRateCommandValidator : AbstractValidator<UpdateLoanRateCommand>
    {
        public UpdateLoanRateCommandValidator()
        {
            RuleFor(a => a.Id).GreaterThan(0);
            RuleFor(a => a.NewAnnualInterestRate).GreaterThan(0);
        }
    }
}
