using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.User
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Debe ingresar su correo electrónico")]
        [Display(Name = "Correo Electrónico")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Formato de correo electrónico inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar su contraseña")]
        [Display(Name = "Nueva Contraseña")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Compare(nameof(Password), ErrorMessage = "La contraseña y la confirmación de contraseña no coinciden.")]
        [Required(ErrorMessage = "Debe confirmar su contraseña")]
        [Display(Name = "Confirmar Contraseña")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;
    }
}
