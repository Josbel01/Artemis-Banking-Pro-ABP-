using MediatR;
using ABP.Core.Application.Dtos.User;
using System.Collections.Generic;

namespace ABP.Core.Application.Features.Users.Queries.GetAllUsers
{
    public class GetAllUsersQuery : IRequest<List<UserDto>>
    {
        public string? Role { get; set; }
    }
}
