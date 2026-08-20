using FluentValidation;

namespace ABP.Core.Application.Features.Account.Commands.ConfirmAccount
{
    public class ConfirmAccountCommandValidator : AbstractValidator<ConfirmAccountCommand>
    {
        public ConfirmAccountCommandValidator()
        {
            RuleFor(p => p.Token)
                .NotEmpty().WithMessage("{PropertyName} es requerido.")
                .NotNull();
        }
    }
}
