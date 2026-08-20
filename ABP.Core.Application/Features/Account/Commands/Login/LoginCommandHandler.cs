using ABP.Core.Application.Dtos.User;
using ABP.Core.Application.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Account.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseApiDto>
    {
        private readonly IAccountServiceWebApi _accountService;

        public LoginCommandHandler(IAccountServiceWebApi accountService)
        {
            _accountService = accountService;
        }

        public async Task<LoginResponseApiDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var dto = new LoginDto 
            { 
                UserName = request.UserName, 
                Password = request.Password 
            };
            return await _accountService.Login(dto);
        }
    }
}
