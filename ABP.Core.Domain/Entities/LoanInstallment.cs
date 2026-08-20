using System;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Common;

namespace ABP.Core.Domain.Entities
{
    public class LoanInstallment : BasicEntity<int>
    {
        public int LoanId { get; set; }
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal InstallmentAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal CapitalAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public bool IsLate { get; set; }
        public DateTime? PaidDate { get; set; }

        public Loan Loan { get; set; } = null!;
    }
}
