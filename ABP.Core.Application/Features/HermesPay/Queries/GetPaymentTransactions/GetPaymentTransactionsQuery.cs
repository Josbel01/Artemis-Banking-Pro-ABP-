using ABP.Core.Application.Dtos.HermesPay;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;

namespace ABP.Core.Application.Features.HermesPay.Queries.GetPaymentTransactions
{
    public class GetPaymentTransactionsQuery : IRequest<PaymentTransactionResponse>
    {
        public int CommerceId { get; set; }
        public string? CommerceUserId { get; set; }

        [SwaggerParameter(Description = "The page number to retrieve", Required = false)]
        public int Page { get; set; } = 1;

        [SwaggerParameter(Description = "The number of records per page", Required = false)]
        public int PageSize { get; set; } = 20;
    }
}
