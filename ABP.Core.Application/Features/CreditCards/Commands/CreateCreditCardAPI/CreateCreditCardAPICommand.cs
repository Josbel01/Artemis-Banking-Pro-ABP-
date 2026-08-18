using MediatR;

namespace ABP.Core.Application.Features.CreditCards.Commands.CreateCreditCardAPI
{
    public class CreateCreditCardAPICommand : IRequest<object>
    {
        public string ClientId { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
    }
}
