using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.User
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Debe ingresar su nombre de usuario")]
        [Display(Name = "Nombre de Usuario")]
        public string UserName { get; set; } = string.Empty;
    }
}
