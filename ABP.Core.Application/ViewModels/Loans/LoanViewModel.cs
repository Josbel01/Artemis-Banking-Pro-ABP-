using ABP.Core.Domain.Common.Enums;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ABP.Core.Application.ViewModels.Loans
{
    public class LoanViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Número de Préstamo")]
        public string LoanNumber { get; set; } = string.Empty;
        
        [Display(Name = "Cliente")]
        public string ClientId { get; set; } = string.Empty;
        
        public string ClientName { get; set; } = string.Empty;
        
        [Display(Name = "Monto Principal")]
        [DataType(DataType.Currency)]
        public decimal PrincipalAmount { get; set; }
        
        [Display(Name = "Monto Pagado")]
        [DataType(DataType.Currency)]
        public decimal AmountPaid { get; set; }
        
        [Display(Name = "Deuda Restante")]
        [DataType(DataType.Currency)]
        public decimal RemainingDebt { get; set; }
        
        [Display(Name = "Cuotas (Meses)")]
        public int TermInMonths { get; set; }
        
        [Display(Name = "Tasa de Interés Anual (%)")]
        public decimal InterestRate { get; set; }
        
        [Display(Name = "Estado")]
        public LoanStatus Status { get; set; }

        public List<LoanInstallmentViewModel> Installments { get; set; } = new List<LoanInstallmentViewModel>();
    }
}
