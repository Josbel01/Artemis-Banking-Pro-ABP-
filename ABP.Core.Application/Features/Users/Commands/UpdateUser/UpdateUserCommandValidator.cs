using FluentValidation;

namespace ABP.Core.Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(p => p.Id).NotEmpty();
            RuleFor(p => p.UserDto).NotNull();
            RuleFor(p => p.UserDto.UserName).NotEmpty();
            RuleFor(p => p.UserDto.Email).NotEmpty().EmailAddress();
            RuleFor(p => p.UserDto.DNI).NotEmpty();
        }
    }
}
