using ABP.Core.Domain.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.SavingAccounts
{
    public class SaveSavingAccountViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe especificar el cliente")]
        public string ClientId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe especificar el monto inicial")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero")]
        public decimal InitialAmount { get; set; }

        public SavingAccountType AccountType { get; set; } = SavingAccountType.Main;
        
        public SavingAccountStatus Status { get; set; } = SavingAccountStatus.Active;
    }
}
