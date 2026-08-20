using FluentValidation;

namespace ABP.Core.Application.Features.Commerces.Commands.CreateCommerceUser
{
    public class CreateCommerceUserCommandValidator : AbstractValidator<CreateCommerceUserCommand>
    {
        public CreateCommerceUserCommandValidator()
        {
            RuleFor(p => p.CommerceId).GreaterThan(0);
            RuleFor(p => p.UserDto).NotNull();
            RuleFor(p => p.UserDto.UserName).NotEmpty();
            RuleFor(p => p.UserDto.Email).NotEmpty().EmailAddress();
            RuleFor(p => p.UserDto.DNI).NotEmpty();
            RuleFor(p => p.UserDto.Password).NotEmpty();
            RuleFor(p => p.UserDto.ConfirmPassword).Equal(p => p.UserDto.Password);
        }
    }
}
