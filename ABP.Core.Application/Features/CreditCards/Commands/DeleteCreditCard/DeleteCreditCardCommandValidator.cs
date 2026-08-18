using FluentValidation;

namespace ABP.Core.Application.Features.CreditCards.Commands.DeleteCreditCard
{
    public class DeleteCreditCardCommandValidator : AbstractValidator<DeleteCreditCardCommand>
    {
        public DeleteCreditCardCommandValidator()
        {
            RuleFor(a => a.Id).GreaterThan(0);
        }
    }
}
