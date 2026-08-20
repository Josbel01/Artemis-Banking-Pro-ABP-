using MediatR;
using ABP.Core.Application.Dtos.User;

namespace ABP.Core.Application.Features.Account.Commands.ConfirmAccount
{
    public class ConfirmAccountCommand : IRequest<UserResponseDto>
    {
        public string UserId { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}
