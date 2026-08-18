using MediatR;

namespace ABP.Core.Application.Features.CreditCards.Queries.GetAllCreditCards
{
    public class GetAllCreditCardsQuery : IRequest<object>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string Status { get; set; } = "activa";
        public string? Identification { get; set; }
    }
}
