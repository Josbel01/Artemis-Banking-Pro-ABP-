using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.ViewModels.User
{
    public class UpdateUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar el nombre")]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar el apellido")]
        [Display(Name = "Apellido")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar un correo electrónico")]
        [Display(Name = "Correo Electrónico")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Formato de correo electrónico inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar un nombre de usuario")]
        [Display(Name = "Nombre de Usuario")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "La contraseña y la confirmación de contraseña no coinciden.")]
        [Display(Name = "Confirmar Contraseña")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Debe ingresar la cédula")]
        [Display(Name = "Cédula")]
        public string Identification { get; set; } = string.Empty;

        public string? Role { get; set; }
        
        [Display(Name = "Monto Inicial (cuando aplique)")]
        public decimal InitialAmount { get; set; }
        
        public bool IsActive { get; set; }
    }
}
