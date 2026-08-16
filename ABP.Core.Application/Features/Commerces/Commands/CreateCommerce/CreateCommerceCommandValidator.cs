using FluentValidation;

namespace ABP.Core.Application.Features.Commerces.Commands.CreateCommerce
{
    public class CreateCommerceCommandValidator : AbstractValidator<CreateCommerceCommand>
    {
        public CreateCommerceCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Commerce name is required.")
                .MaximumLength(150).WithMessage("Commerce name must not exceed 150 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Commerce email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.Rnc)
                .NotEmpty().WithMessage("Commerce RNC is required.")
                .MaximumLength(20).WithMessage("RNC must not exceed 20 characters.");
        }
    }
}