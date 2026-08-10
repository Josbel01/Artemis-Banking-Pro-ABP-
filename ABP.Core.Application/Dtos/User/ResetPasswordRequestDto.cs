namespace ABP.Core.Application.Dtos.User
{
    public class ResetPasswordRequestDto
    {
        public required string Email { get; set; } = null!;
        public required string Password { get; set; } = null!;
        public required string ConfirmPassword { get; set; } = null!;
        public required string Token { get; set; } = null!;
    }
}
