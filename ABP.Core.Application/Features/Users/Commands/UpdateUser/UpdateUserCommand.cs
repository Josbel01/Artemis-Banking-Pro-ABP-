using MediatR;
using ABP.Core.Application.Dtos.User;

namespace ABP.Core.Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommand : IRequest<EditResponseDto>
    {
        public string Id { get; set; } = null!;
        public SaveUserDto UserDto { get; set; } = null!;
    }
}
