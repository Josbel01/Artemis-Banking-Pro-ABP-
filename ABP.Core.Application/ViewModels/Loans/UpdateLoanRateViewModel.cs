using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.Loans
{
    public class UpdateLoanRateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La tasa de interés anual es requerida.")]
        [Range(0, 100, ErrorMessage = "La tasa de interés anual debe ser mayor o igual a cero.")]
        [Display(Name = "Tasa de Interés Anual")]
        public decimal AnnualInterestRate { get; set; }
    }
}
