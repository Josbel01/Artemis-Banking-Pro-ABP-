using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.Cashier
{
    public class WithdrawalViewModel
    {
        [Required(ErrorMessage = "El número de cuenta es requerido.")]
        [Display(Name = "Número de Cuenta")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es requerido.")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a RD$0.")]
        [Display(Name = "Monto a Retirar (RD$)")]
        public decimal Amount { get; set; }
    }
}
