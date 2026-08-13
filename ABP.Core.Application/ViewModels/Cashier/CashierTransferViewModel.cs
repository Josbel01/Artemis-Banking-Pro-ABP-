using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.Cashier
{
    public class CashierTransferViewModel
    {
        [Required(ErrorMessage = "La cuenta de origen es requerida.")]
        [Display(Name = "Cuenta de Origen")]
        public string OriginAccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cuenta de destino es requerida.")]
        [Display(Name = "Cuenta de Destino")]
        public string DestinationAccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es requerido.")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a RD$0.")]
        [Display(Name = "Monto a Transferir (RD$)")]
        public decimal Amount { get; set; }
    }
}
