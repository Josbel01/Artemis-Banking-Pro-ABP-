using ABP.Core.Domain.Common.Enums;
using System.Collections.Generic;

namespace ABP.Core.Application.Dtos.Loans
{
    public class LoanDto : BasicDto<int>
    {
        public string LoanNumber { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public decimal AmountApproved { get; set; }
        public decimal AmountPending { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public int TermInMonths { get; set; }
        public LoanStatus Status { get; set; }
        public string AssignedByUserId { get; set; } = string.Empty;

        public int TotalInstallments { get; set; }
        public int PaidInstallments { get; set; }
        public ClientPaymentStatus ClientPaymentStatus { get; set; }

        public ICollection<LoanInstallmentDto> LoanInstallments { get; set; } = new List<LoanInstallmentDto>();
    }
}
