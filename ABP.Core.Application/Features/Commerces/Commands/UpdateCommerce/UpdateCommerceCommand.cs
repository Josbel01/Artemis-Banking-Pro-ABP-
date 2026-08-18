using MediatR;

namespace ABP.Core.Application.Features.Commerces.Commands.UpdateCommerce
{
    public class UpdateCommerceCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Rnc { get; set; } = string.Empty;
    }
}
