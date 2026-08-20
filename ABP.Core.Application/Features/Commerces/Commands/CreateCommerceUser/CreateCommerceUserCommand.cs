using MediatR;
using ABP.Core.Application.Dtos.User;

namespace ABP.Core.Application.Features.Commerces.Commands.CreateCommerceUser
{
    public class CreateCommerceUserCommand : IRequest<RegisterResponseDto>
    {
        public int CommerceId { get; set; }
        public SaveUserDto UserDto { get; set; } = null!;
        public string? Origin { get; set; }
    }
}
