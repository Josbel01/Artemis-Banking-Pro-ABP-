using FluentValidation;

namespace ABP.Core.Application.Features.Loans.Commands.CreateLoan
{
    public class CreateLoanCommandValidator : AbstractValidator<CreateLoanCommand>
    {
        public CreateLoanCommandValidator()
        {
            RuleFor(x => x.ClientId).NotEmpty().NotNull();
            RuleFor(x => x.CapitalAmount).GreaterThan(0);
            RuleFor(x => x.TermInMonths).GreaterThan(0);
        }
    }
}
