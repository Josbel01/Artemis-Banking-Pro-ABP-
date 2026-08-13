using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.User
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Debe ingresar su correo electrónico")]
        [Display(Name = "Correo Electrónico")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Formato de correo electrónico inválido")]
        public string Email { get; set; } = string.Empty;
    }
}
