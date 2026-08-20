using ABP.Core.Application.Dtos.User;
using MediatR;

namespace ABP.Core.Application.Features.Account.Commands.Login
{
    public class LoginCommand : IRequest<LoginResponseApiDto>
    {
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
