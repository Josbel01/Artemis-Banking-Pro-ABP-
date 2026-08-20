using FluentValidation;

namespace ABP.Core.Application.Features.CreditCards.Commands.CreateCreditCard
{
    public class CreateCreditCardCommandValidator : AbstractValidator<CreateCreditCardCommand>
    {
        public CreateCreditCardCommandValidator()
        {
            RuleFor(x => x.CardNumber)
                .NotEmpty().WithMessage("Card number is required.")
                .Length(16).WithMessage("Card number must be 16 characters.");

            RuleFor(x => x.ClientId)
                .NotEmpty().WithMessage("Client ID is required.");

            RuleFor(x => x.CreditLimit)
                .GreaterThan(0).WithMessage("Credit limit must be greater than 0.");

            RuleFor(x => x.ExpirationDate)
                .NotEmpty().WithMessage("Expiration date is required.")
                .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future.");

            RuleFor(x => x.Cvc)
                .NotEmpty().WithMessage("CVC is required.")
                .Length(3).WithMessage("CVC must be 3 digits.");

            RuleFor(x => x.AssignedByUserId)
                .NotEmpty().WithMessage("Admin assigned ID is required.");
        }
    }
}