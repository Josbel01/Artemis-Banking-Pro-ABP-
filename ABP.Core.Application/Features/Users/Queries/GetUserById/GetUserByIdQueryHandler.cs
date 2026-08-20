using MediatR;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Dtos.User;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
    {
        private readonly IAccountServiceWebApi _accountService;

        public GetUserByIdQueryHandler(IAccountServiceWebApi accountService)
        {
            _accountService = accountService;
        }

        public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            return await _accountService.GetUserById(request.Id);
        }
    }
}
