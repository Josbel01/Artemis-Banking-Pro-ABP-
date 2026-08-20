using MediatR;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Dtos.User;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Account.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, UserResponseDto>
    {
        private readonly IAccountServiceWebApi _accountService;

        public ForgotPasswordCommandHandler(IAccountServiceWebApi accountService)
        {
            _accountService = accountService;
        }

        public async Task<UserResponseDto> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var dto = new ForgotPasswordRequestDto 
            { 
                UserName = request.UserName,
                Origin = request.Origin
            };
            return await _accountService.ForgotPasswordAsync(dto, true);
        }
    }
}
