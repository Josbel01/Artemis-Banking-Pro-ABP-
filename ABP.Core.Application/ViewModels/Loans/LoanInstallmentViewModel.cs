using ABP.Core.Domain.Common.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.Loans
{
    public class LoanInstallmentViewModel
    {
        public int Id { get; set; }
        
        public int LoanId { get; set; }
        
        [Display(Name = "Número de Cuota")]
        public int InstallmentNumber { get; set; }
        
        [Display(Name = "Fecha de Pago")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }
        
        [Display(Name = "Monto de Cuota")]
        [DataType(DataType.Currency)]
        public decimal InstallmentAmount { get; set; }
        
        [Display(Name = "Capital")]
        [DataType(DataType.Currency)]
        public decimal CapitalAmount { get; set; }
        
        [Display(Name = "Interés")]
        [DataType(DataType.Currency)]
        public decimal InterestAmount { get; set; }
        
        [Display(Name = "Monto Pendiente")]
        [DataType(DataType.Currency)]
        public decimal PendingAmount { get; set; }
        
        [Display(Name = "Estado")]
        public PaymentStatus PaymentStatus { get; set; }
        
        public bool IsLate { get; set; }
    }
}
