using System;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Common;

namespace ABP.Core.Domain.Entities
{
    public class CardTransaction : BasicEntity<int>
    {
        public int CreditCardId { get; set; }
        public int? CommerceId { get; set; } // Opcional, porque un Avance de efectivo no tiene comercio asociado
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string CommerceName { get; set; } = string.Empty;
        public TransactionStatus Status { get; set; }

        public CreditCard CreditCard { get; set; } = null!;
        public Commerce? Commerce { get; set; }
    }
}
