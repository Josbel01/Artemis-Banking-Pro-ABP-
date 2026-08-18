using FluentValidation;

namespace ABP.Core.Application.Features.SavingAccounts.Commands.DeleteSavingAccount
{
    public class DeleteSavingAccountCommandValidator : AbstractValidator<DeleteSavingAccountCommand>
    {
        public DeleteSavingAccountCommandValidator()
        {
            RuleFor(a => a.Id).GreaterThan(0);
        }
    }
}
