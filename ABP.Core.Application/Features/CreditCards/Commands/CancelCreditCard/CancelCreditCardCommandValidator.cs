using FluentValidation;

namespace ABP.Core.Application.Features.CreditCards.Commands.CancelCreditCard
{
    public class CancelCreditCardCommandValidator : AbstractValidator<CancelCreditCardCommand>
    {
        public CancelCreditCardCommandValidator()
        {
            RuleFor(a => a.Id).GreaterThan(0);
        }
    }
}
