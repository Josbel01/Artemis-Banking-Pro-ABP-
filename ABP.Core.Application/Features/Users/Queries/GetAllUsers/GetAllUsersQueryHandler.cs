using MediatR;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Dtos.User;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ABP.Core.Application.Features.Users.Queries.GetAllUsers
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
    {
        private readonly IAccountServiceWebApi _accountService;

        public GetAllUsersQueryHandler(IAccountServiceWebApi accountService)
        {
            _accountService = accountService;
        }

        public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _accountService.GetAllUser(null);
            var validUsers = users.Where(u => u.Roles != null && !u.Roles.Contains(ABP.Core.Domain.Common.Enums.UserRoles.Commerce.ToString())).ToList();

            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                validUsers = validUsers.Where(u => u.Roles != null && u.Roles.Contains(request.Role, StringComparer.OrdinalIgnoreCase)).ToList();
            }

            return validUsers.OrderByDescending(u => u.Id).ToList();
        }
    }
}
