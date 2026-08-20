using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.Transactions
{
    public class SaveLoanPaymentViewModel
    {
        [Required(ErrorMessage = "El préstamo a pagar es requerido")]
        public string LoanNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cuenta de origen es requerida")]
        public string OriginAccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto a pagar es requerido")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }
    }
}
