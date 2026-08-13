using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.User
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Debe ingresar su nombre")]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar su apellido")]
        [Display(Name = "Apellido")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar su correo electrónico")]
        [Display(Name = "Correo Electrónico")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Formato de correo electrónico inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar un nombre de usuario")]
        [Display(Name = "Nombre de Usuario")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar su contraseña")]
        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Compare(nameof(Password), ErrorMessage = "La contraseña y la confirmación de contraseña no coinciden.")]
        [Required(ErrorMessage = "Debe confirmar su contraseña")]
        [Display(Name = "Confirmar Contraseña")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar su cédula")]
        [Display(Name = "Cédula")]
        public string Identification { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un tipo de usuario")]
        [Display(Name = "Tipo de Usuario")]
        public string UserType { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
