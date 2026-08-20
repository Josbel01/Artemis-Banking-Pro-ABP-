using FluentValidation;

namespace ABP.Core.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(p => p.UserDto).NotNull();
            RuleFor(p => p.UserDto.UserName).NotEmpty();
            RuleFor(p => p.UserDto.Email).NotEmpty().EmailAddress();
            RuleFor(p => p.UserDto.DNI).NotEmpty();
            RuleFor(p => p.UserDto.Role).NotEmpty().Must(r => r != "Commerce").WithMessage("No se puede crear un usuario con rol Comercio desde este endpoint.");
            RuleFor(p => p.UserDto.Password).NotEmpty();
            RuleFor(p => p.UserDto.ConfirmPassword).Equal(p => p.UserDto.Password);
        }
    }
}
