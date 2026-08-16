using FluentValidation;

namespace ABP.Core.Application.Features.Commerces.Commands.UpdateCommerce
{
    public class UpdateCommerceCommandValidator : AbstractValidator<UpdateCommerceCommand>
    {
        public UpdateCommerceCommandValidator()
        {
            RuleFor(a => a.Id).GreaterThan(0).WithMessage("Commerce ID must be greater than 0.");
            RuleFor(a => a.Name).NotEmpty().MaximumLength(150);
            RuleFor(a => a.Email).NotEmpty().EmailAddress();
            RuleFor(a => a.PhoneNumber).NotEmpty();
            RuleFor(a => a.Rnc).NotEmpty().MaximumLength(11);
        }
    }
}
