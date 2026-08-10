using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.Dtos.User
{
    public class ForgotPasswordApiRequestDto
    {
        [Required(ErrorMessage = "El correo electrónico o nombre de usuario es requerido")]
        public required string EmailOrUserName { get; set; } = null!;

        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? Origin { get; set; }
    }
}
