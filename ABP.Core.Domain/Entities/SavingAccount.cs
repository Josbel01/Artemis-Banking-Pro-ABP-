using System;
using System.Collections.Generic;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Common;

namespace ABP.Core.Domain.Entities
{
    public class SavingAccount : BasicEntity<int>
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public SavingAccountType AccountType { get; set; }
        public SavingAccountStatus Status { get; set; }

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
