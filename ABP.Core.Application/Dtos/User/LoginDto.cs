namespace ABP.Core.Application.Dtos.User
{
    public class LoginDto
    {
        public required string UserName { get; set; } = null!;
        public required string Password { get; set; } = null!;
        public bool RememberMe { get; set; }
    }
}
