using System;
using ABP.Core.Domain.Common.Enums;

namespace ABP.Core.Application.Dtos.Transactions
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public int SavingAccountId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string Beneficiary { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public TransactionStatus Status { get; set; }
        public string? ResponsibleUserId { get; set; }

        public Dtos.SavingAccounts.SavingAccountDto? SavingAccount { get; set; }
    }
}
