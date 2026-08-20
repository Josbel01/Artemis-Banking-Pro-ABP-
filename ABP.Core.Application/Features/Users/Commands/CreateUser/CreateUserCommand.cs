using MediatR;
using ABP.Core.Application.Dtos.User;

namespace ABP.Core.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommand : IRequest<RegisterResponseDto>
    {
        public SaveUserDto UserDto { get; set; } = null!;
        public string? Origin { get; set; }
    }
}
