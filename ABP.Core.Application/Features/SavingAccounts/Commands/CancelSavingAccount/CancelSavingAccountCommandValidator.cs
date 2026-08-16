using FluentValidation;

namespace ABP.Core.Application.Features.SavingAccounts.Commands.CancelSavingAccount
{
    public class CancelSavingAccountCommandValidator : AbstractValidator<CancelSavingAccountCommand>
    {
        public CancelSavingAccountCommandValidator()
        {
            RuleFor(a => a.Id).GreaterThan(0);
        }
    }
}
