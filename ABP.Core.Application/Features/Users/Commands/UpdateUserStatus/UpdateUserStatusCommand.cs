using MediatR;
namespace ABP.Core.Application.Features.Users.Commands.UpdateUserStatus
{
    public class UpdateUserStatusCommand : IRequest<bool>
    {
        public string Id { get; set; } = null!;
        public bool Status { get; set; }
    }
}
