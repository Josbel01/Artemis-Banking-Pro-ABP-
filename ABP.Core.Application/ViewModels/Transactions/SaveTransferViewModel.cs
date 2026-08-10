using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.Transactions
{
    public class SaveTransferViewModel
    {
        [Required(ErrorMessage = "La cuenta de origen es requerida")]
        public string OriginAccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cuenta destino es requerida")]
        public string DestinationAccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto a transferir es requerido")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }
    }
}
