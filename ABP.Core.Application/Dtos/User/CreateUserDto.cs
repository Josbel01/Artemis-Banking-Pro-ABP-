using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.Dtos.User
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public required string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es requerido")]
        public required string LastName { get; set; } = null!;

        [Required(ErrorMessage = "La cédula de identidad es requerida")]
        public required string DNI { get; set; } = null!;

        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido")]
        public required string Email { get; set; } = null!;

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public required string UserName { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        public required string Password { get; set; } = null!;

        [Required(ErrorMessage = "Debe confirmar la contraseña")]
        [Compare(nameof(Password), ErrorMessage = "La contraseña y la confirmación no coinciden")]
        [DataType(DataType.Password)]
        public required string ConfirmPassword { get; set; } = null!;

        public string? PhoneNumber { get; set; }
        public string? PhotoUrl { get; set; }

        public string? Role { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
