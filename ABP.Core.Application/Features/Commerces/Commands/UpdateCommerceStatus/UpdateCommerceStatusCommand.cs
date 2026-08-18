using MediatR;

namespace ABP.Core.Application.Features.Commerces.Commands.UpdateCommerceStatus
{
    public class UpdateCommerceStatusCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public bool Status { get; set; }
    }
}
