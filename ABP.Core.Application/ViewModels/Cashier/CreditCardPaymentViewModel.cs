using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.Cashier
{
    public class CreditCardPaymentViewModel
    {
        [Required(ErrorMessage = "El número de tarjeta es requerido.")]
        [Display(Name = "Número de Tarjeta")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es requerido.")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a RD$0.")]
        [Display(Name = "Monto a Pagar (RD$)")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "El número de cuenta de ahorro es requerido.")]
        [Display(Name = "Cuenta de Ahorro Origen")]
        public string OriginAccountNumber { get; set; } = string.Empty;
    }
}
