using ABP.Core.Domain.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.Loans
{
    public class SaveLoanViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe especificar el cliente")]
        public string ClientId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe especificar el monto del préstamo")]
        [Range(1000, double.MaxValue, ErrorMessage = "El monto debe ser un valor válido")]
        public decimal PrincipalAmount { get; set; }

        [Required(ErrorMessage = "Debe especificar la cantidad de cuotas (meses)")]
        [Range(1, 360, ErrorMessage = "La cantidad de cuotas debe estar entre 1 y 360")]
        public int TermInMonths { get; set; }

        [Required(ErrorMessage = "Debe especificar la tasa de interés anual")]
        [Range(0.1, 100, ErrorMessage = "La tasa de interés debe ser válida")]
        public decimal InterestRate { get; set; }

        public LoanStatus Status { get; set; } = LoanStatus.Active;
    }
}
