using ABP.Core.Domain.Common.Enums;
using System.Collections.Generic;

namespace ABP.Core.Application.Dtos.SavingAccounts
{
    public class SavingAccountDto : BasicDto<int>
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public SavingAccountType AccountType { get; set; }
        public SavingAccountStatus Status { get; set; }
    }
}
