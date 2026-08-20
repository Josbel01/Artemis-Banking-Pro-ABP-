using MediatR;
using ABP.Core.Application.Dtos.User;

namespace ABP.Core.Application.Features.Account.Commands.ForgotPassword
{
    public class ForgotPasswordCommand : IRequest<UserResponseDto>
    {
        public string UserName { get; set; } = null!;
        public string? Origin { get; set; }
    }
}
