using ABP.Core.Domain.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.SavingAccounts
{
    public class SavingAccountViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Número de Cuenta")]
        public string AccountNumber { get; set; } = string.Empty;
        
        [Display(Name = "Cliente")]
        public string ClientId { get; set; } = string.Empty;
        
        public string ClientName { get; set; } = string.Empty; // Useful for UI display

        [Display(Name = "Asignado por")]
        public string AdminName { get; set; } = string.Empty;

        [Display(Name = "Asignado por")]
        public string AssignedByUserId { get; set; } = string.Empty;
        
        [Display(Name = "Balance")]
        [DataType(DataType.Currency)]
        public decimal Balance { get; set; }
        
        [Display(Name = "Tipo de Cuenta")]
        public SavingAccountType AccountType { get; set; }
        
        [Display(Name = "Estado")]
        public SavingAccountStatus Status { get; set; }

        [Display(Name = "Fecha de Creación")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }
    }
}
