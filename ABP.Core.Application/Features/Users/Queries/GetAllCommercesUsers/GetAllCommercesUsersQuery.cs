using MediatR;
using ABP.Core.Application.Dtos.User;
using System.Collections.Generic;

namespace ABP.Core.Application.Features.Users.Queries.GetAllCommercesUsers
{
    public class GetAllCommercesUsersQuery : IRequest<List<UserDto>>
    {
    }
}
