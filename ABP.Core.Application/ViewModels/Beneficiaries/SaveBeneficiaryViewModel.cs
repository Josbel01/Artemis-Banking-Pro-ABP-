using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.Beneficiaries
{
    public class SaveBeneficiaryViewModel
    {
        public int Id { get; set; }

        // ClientId is set server-side, no client validation needed
        public string ClientId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de cuenta es requerido")]
        public string BeneficiaryAccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es requerido")]
        public string BeneficiaryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        public string BeneficiaryLastName { get; set; } = string.Empty;
    }
}
