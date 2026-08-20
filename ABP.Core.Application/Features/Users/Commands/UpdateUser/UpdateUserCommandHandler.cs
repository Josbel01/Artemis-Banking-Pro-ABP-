using MediatR;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Dtos.User;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, EditResponseDto>
    {
        private readonly IAccountServiceWebApi _accountService;

        public UpdateUserCommandHandler(IAccountServiceWebApi accountService)
        {
            _accountService = accountService;
        }

        public async Task<EditResponseDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            return await _accountService.EditUser(request.UserDto, null, false, true);
        }
    }
}
