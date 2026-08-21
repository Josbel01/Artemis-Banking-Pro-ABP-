using MediatR;
using Swashbuckle.AspNetCore.Annotations;

namespace ABP.Core.Application.Features.HermesPay.Commands.ProcessPayment
{
    public class ProcessPaymentCommand : IRequest
    {
        [SwaggerParameter(Description = "Commerce ID or use CommerceUserId instead")]
        public int CommerceId { get; set; }

        [SwaggerParameter(Description = "Commerce user ID (alternative to CommerceId)")]
        public string? CommerceUserId { get; set; }

        [SwaggerParameter(Description = "The 16-digit credit card number")]
        public string CardNumber { get; set; } = string.Empty;

        [SwaggerParameter(Description = "The 2-digit expiration month (MM)")]
        public string MonthExpirationCard { get; set; } = string.Empty;

        [SwaggerParameter(Description = "The 4-digit expiration year (YYYY)")]
        public string YearExpirationCard { get; set; } = string.Empty;

        [SwaggerParameter(Description = "The 3-digit security code")]
        public string Cvc { get; set; } = string.Empty;

        [SwaggerParameter(Description = "The amount to be processed")]
        public decimal TransactionAmount { get; set; }
    }
}
