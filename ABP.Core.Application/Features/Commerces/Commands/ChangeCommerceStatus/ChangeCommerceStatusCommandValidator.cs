using FluentValidation;

namespace ABP.Core.Application.Features.Commerces.Commands.ChangeCommerceStatus
{
    public class ChangeCommerceStatusCommandValidator : AbstractValidator<ChangeCommerceStatusCommand>
    {
        public ChangeCommerceStatusCommandValidator()
        {
            RuleFor(a => a.Id).GreaterThan(0);
        }
    }
}
