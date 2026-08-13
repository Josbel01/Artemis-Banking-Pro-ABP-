using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.User
{
    public class ResendActivationViewModel
    {
        [Required(ErrorMessage = "Debe ingresar su nombre de usuario")]
        [Display(Name = "Nombre de Usuario")]
        [DataType(DataType.Text)]
        public string UserName { get; set; } = string.Empty;
    }
}
