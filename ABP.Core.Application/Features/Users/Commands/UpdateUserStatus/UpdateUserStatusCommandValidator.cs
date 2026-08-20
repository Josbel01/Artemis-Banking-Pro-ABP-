using FluentValidation;

namespace ABP.Core.Application.Features.Users.Commands.UpdateUserStatus
{
    public class UpdateUserStatusCommandValidator : AbstractValidator<UpdateUserStatusCommand>
    {
        public UpdateUserStatusCommandValidator()
        {
            RuleFor(p => p.Id).NotEmpty();
            RuleFor(p => p.Status).NotNull();
        }
    }
}
