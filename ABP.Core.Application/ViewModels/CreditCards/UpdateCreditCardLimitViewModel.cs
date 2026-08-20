using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.CreditCards
{
    public class UpdateCreditCardLimitViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El límite de crédito es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El límite de crédito debe ser mayor que cero.")]
        [Display(Name = "Límite de Crédito")]
        [DataType(DataType.Currency)]
        public decimal CreditLimit { get; set; }
    }
}
