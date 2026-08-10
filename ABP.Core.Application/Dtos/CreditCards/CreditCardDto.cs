using ABP.Core.Domain.Common.Enums;

namespace ABP.Core.Application.Dtos.CreditCards
{
    public class CreditCardDto : BasicDto<int>
    {
        public string CardNumber { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public decimal CurrentDebt { get; set; }
        public string ExpirationDate { get; set; } = string.Empty; 
        public string Cvc { get; set; } = string.Empty;
        public CreditCardStatus Status { get; set; }
        public string AssignedByUserId { get; set; } = string.Empty;
    }
}
