using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.Dtos.User
{
    public class ConfirmAccountRequestDto
    {
        [Required(ErrorMessage = "El ID de usuario es requerido")]
        public required string UserId { get; set; }

        [Required(ErrorMessage = "El token de confirmación es requerido")]
        public required string Token { get; set; }
    }
}
