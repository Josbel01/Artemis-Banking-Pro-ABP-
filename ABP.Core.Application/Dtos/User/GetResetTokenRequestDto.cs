using System.ComponentModel.DataAnnotations;

namespace ABP.Core.Application.Dtos.User
{
    public class GetResetTokenRequestDto
    {
        [Required]
        public string UserName { get; set; } = string.Empty;
    }
}
