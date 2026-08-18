using MediatR;

namespace ABP.Core.Application.Features.CreditCards.Commands.DeleteCreditCardAPI
{
    public class DeleteCreditCardAPICommand : IRequest<bool>
    {
        public string Id { get; set; } = string.Empty;
    }
}
