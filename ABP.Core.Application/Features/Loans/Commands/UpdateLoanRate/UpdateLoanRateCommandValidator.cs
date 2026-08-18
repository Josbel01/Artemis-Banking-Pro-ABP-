using FluentValidation;

namespace ABP.Core.Application.Features.Loans.Commands.UpdateLoanRate
{
    public class UpdateLoanRateCommandValidator : AbstractValidator<UpdateLoanRateCommand>
    {
        public UpdateLoanRateCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().NotNull();
            RuleFor(x => x.AnnualInterestRate).GreaterThan(0);
        }
    }
}
