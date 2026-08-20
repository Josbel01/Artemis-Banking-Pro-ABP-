using MediatR;
using ABP.Core.Application.Dtos.User;

namespace ABP.Core.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQuery : IRequest<UserDto?>
    {
        public string Id { get; set; } = null!;
    }
}
