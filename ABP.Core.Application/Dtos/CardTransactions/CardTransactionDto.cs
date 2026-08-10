using ABP.Core.Domain.Common.Enums;
using System;

namespace ABP.Core.Application.Dtos.CardTransactions
{
    public class CardTransactionDto : BasicDto<int>
    {
        public int CreditCardId { get; set; }
        public int? CommerceId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string CommerceName { get; set; } = string.Empty;
        public TransactionStatus Status { get; set; }
    }
}
