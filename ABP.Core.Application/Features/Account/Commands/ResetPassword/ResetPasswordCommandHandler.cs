using MediatR;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Dtos.User;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ABP.Core.Application.Features.Account.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, UserResponseDto>
    {
        private readonly IAccountServiceWebApi _accountService;

        public ResetPasswordCommandHandler(IAccountServiceWebApi accountService)
        {
            _accountService = accountService;
        }

        public async Task<UserResponseDto> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _accountService.GetUserById(request.UserId);
            if (user == null)
            {
                return new UserResponseDto { HasError = true, Errors = new List<string> { "El usuario indicado no existe." } };
            }

            var dto = new ResetPasswordRequestDto 
            { 
                Email = user.Email,
                Token = request.Token,
                Password = request.Password,
                ConfirmPassword = request.ConfirmPassword
            };
            return await _accountService.ResetPasswordAsync(dto);
        }
    }
}
