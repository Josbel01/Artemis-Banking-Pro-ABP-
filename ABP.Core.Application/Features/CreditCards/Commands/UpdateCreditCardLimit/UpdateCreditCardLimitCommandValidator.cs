using FluentValidation;

namespace ABP.Core.Application.Features.CreditCards.Commands.UpdateCreditCardLimit
{
    public class UpdateCreditCardLimitCommandValidator : AbstractValidator<UpdateCreditCardLimitCommand>
    {
        public UpdateCreditCardLimitCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().NotNull();
            RuleFor(a => a.CreditLimit).GreaterThan(0);
        }
    }
}
