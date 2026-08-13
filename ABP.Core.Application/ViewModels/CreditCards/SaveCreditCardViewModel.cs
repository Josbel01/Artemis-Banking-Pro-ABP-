using ABP.Core.Domain.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.CreditCards
{
    public class SaveCreditCardViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe especificar el cliente")]
        public string ClientId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe especificar el límite de crédito")]
        [Range(100, double.MaxValue, ErrorMessage = "El límite debe ser un monto válido")]
        public decimal CreditLimit { get; set; }

        public CreditCardStatus Status { get; set; } = CreditCardStatus.Active;
    }
}
