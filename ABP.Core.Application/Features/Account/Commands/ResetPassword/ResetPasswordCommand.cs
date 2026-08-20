using MediatR;
using ABP.Core.Application.Dtos.User;

namespace ABP.Core.Application.Features.Account.Commands.ResetPassword
{
    public class ResetPasswordCommand : IRequest<UserResponseDto>
    {
        public string UserId { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }
}
