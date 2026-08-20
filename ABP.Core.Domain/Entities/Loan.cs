using System;
using System.Collections.Generic;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Common;

namespace ABP.Core.Domain.Entities
{
    public class Loan : BasicEntity<int>
    {
        public string LoanNumber { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public decimal AmountApproved { get; set; }
        public decimal AmountPending { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public int TermInMonths { get; set; }
        public LoanStatus Status { get; set; }
        public string AssignedByUserId { get; set; } = string.Empty; // Administrador responsable de la asignación

        public int TotalInstallments { get; set; }
        public int PaidInstallments { get; set; }
        public ClientPaymentStatus ClientPaymentStatus { get; set; }
        
        public ICollection<LoanInstallment> LoanInstallments { get; set; } = new List<LoanInstallment>();
    }
}
