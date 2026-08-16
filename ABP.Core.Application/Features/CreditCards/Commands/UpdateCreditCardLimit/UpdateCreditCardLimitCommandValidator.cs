using FluentValidation;

namespace ABP.Core.Application.Features.CreditCards.Commands.UpdateCreditCardLimit
{
    public class UpdateCreditCardLimitCommandValidator : AbstractValidator<UpdateCreditCardLimitCommand>
    {
        public UpdateCreditCardLimitCommandValidator()
        {
            RuleFor(a => a.Id).GreaterThan(0);
            RuleFor(a => a.NewCreditLimit).GreaterThan(0);
        }
    }
}
