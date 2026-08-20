using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.SavingAccounts
{
    public class SaveSavingAccountViewModel
    {
        [Required(ErrorMessage = "El cliente seleccionado es requerido.")]
        public string ClientId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El balance inicial es requerido.")]
        [Range(0, double.MaxValue, ErrorMessage = "El balance inicial no puede ser negativo.")]
        [Display(Name = "Balance Inicial")]
        [DataType(DataType.Currency)]
        public decimal InitialBalance { get; set; }
    }
}
