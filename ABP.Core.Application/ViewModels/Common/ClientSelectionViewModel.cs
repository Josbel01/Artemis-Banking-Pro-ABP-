using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.Common
{
    /// <summary>
    /// ViewModel for displaying client info in the selection table (Step 1).
    /// </summary>
    public class ClientSelectionViewModel
    {
        public string ClientId { get; set; } = string.Empty;

        [Display(Name = "Nombre y apellido")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Correo electr\u00f3nico")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "C\u00e9dula")]
        public string DNI { get; set; } = string.Empty;

        [Display(Name = "Monto total de deuda")]
        [DataType(DataType.Currency)]
        public decimal TotalDebt { get; set; }
    }

    /// <summary>
    /// ViewModel for submitting the selected client from Step 1.
    /// </summary>
    public class ClientSelectionInputViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un cliente para continuar.")]
        public string? SelectedClientId { get; set; }
    }
}
