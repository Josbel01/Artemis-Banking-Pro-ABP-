using FluentValidation;

namespace ABP.Core.Application.Features.SavingAccounts.Commands.CreateSavingAccount
{
    public class CreateSavingAccountCommandValidator : AbstractValidator<CreateSavingAccountCommand>
    {
        public CreateSavingAccountCommandValidator()
        {
            RuleFor(x => x.AccountNumber)
                .NotEmpty().WithMessage("Account number is required.")
                .Length(9).WithMessage("Account number must be 9 digits.");

            RuleFor(x => x.ClientId)
                .NotEmpty().WithMessage("Client ID is required.");

            RuleFor(x => x.Balance)
                .GreaterThanOrEqualTo(0).WithMessage("Balance cannot be negative.");

            RuleFor(x => x.AccountType)
                .IsInEnum().WithMessage("Invalid saving account type.");
        }
    }
}