using MediatR;

namespace ABP.Core.Application.Features.Commerces.Commands.CreateCommerce
{
    public class CreateCommerceCommand : IRequest<object>
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Rnc { get; set; } = string.Empty;
    }
}
