using MediatR;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Dtos.User;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Account.Commands.ConfirmAccount
{
    public class ConfirmAccountCommandHandler : IRequestHandler<ConfirmAccountCommand, UserResponseDto>
    {
        private readonly IAccountServiceWebApi _accountService;

        public ConfirmAccountCommandHandler(IAccountServiceWebApi accountService)
        {
            _accountService = accountService;
        }

        public async Task<UserResponseDto> Handle(ConfirmAccountCommand request, CancellationToken cancellationToken)
        {
            return await _accountService.ConfirmAccountAsync(request.UserId, request.Token);
        }
    }
}
