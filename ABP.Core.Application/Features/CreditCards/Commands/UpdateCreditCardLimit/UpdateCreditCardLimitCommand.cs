using MediatR;

namespace ABP.Core.Application.Features.CreditCards.Commands.UpdateCreditCardLimit
{
    public class UpdateCreditCardLimitCommand : IRequest<bool>
    {
        public string Id { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
    }
}
