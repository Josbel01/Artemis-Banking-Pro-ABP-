using MediatR;

namespace ABP.Core.Application.Features.CreditCards.Queries.GetCreditCardById
{
    public class GetCreditCardByIdQuery : IRequest<object>
    {
        public string Id { get; set; } = string.Empty;
    }
}
