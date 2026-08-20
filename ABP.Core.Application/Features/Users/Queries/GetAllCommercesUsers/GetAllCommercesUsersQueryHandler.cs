using MediatR;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Dtos.User;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace ABP.Core.Application.Features.Users.Queries.GetAllCommercesUsers
{
    public class GetAllCommercesUsersQueryHandler : IRequestHandler<GetAllCommercesUsersQuery, List<UserDto>>
    {
        private readonly IAccountServiceWebApi _accountService;

        public GetAllCommercesUsersQueryHandler(IAccountServiceWebApi accountService)
        {
            _accountService = accountService;
        }

        public async Task<List<UserDto>> Handle(GetAllCommercesUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _accountService.GetAllUser(null);
            return users.Where(u => u.Roles != null && u.Roles.Contains(ABP.Core.Domain.Common.Enums.UserRoles.Commerce.ToString()))
                        .OrderByDescending(u => u.Id)
                        .ToList();
        }
    }
}
