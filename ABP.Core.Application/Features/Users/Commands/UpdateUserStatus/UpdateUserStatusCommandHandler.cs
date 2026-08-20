using MediatR;
using ABP.Core.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Users.Commands.UpdateUserStatus
{
    public class UpdateUserStatusCommandHandler : IRequestHandler<UpdateUserStatusCommand, bool>
    {
        private readonly IAccountServiceWebApi _accountService;

        public UpdateUserStatusCommandHandler(IAccountServiceWebApi accountService)
        {
            _accountService = accountService;
        }

        public async Task<bool> Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
        {
            return await _accountService.UpdateUserStatusAsync(request.Id, request.Status);
        }
    }
}
