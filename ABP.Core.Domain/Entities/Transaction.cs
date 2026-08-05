using System;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Common;

namespace ABP.Core.Domain.Entities
{
    public class Transaction : BasicEntity<int>
    {
        public int SavingAccountId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string Beneficiary { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public TransactionStatus Status { get; set; }
        public string? ResponsibleUserId { get; set; } // Cajero autenticado o null si fue el cliente

        public SavingAccount SavingAccount { get; set; } = null!;
    }
}
