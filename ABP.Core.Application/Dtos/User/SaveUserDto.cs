namespace ABP.Core.Application.Dtos.User
{
    public class SaveUserDto
    {
        public required string? Id { get; set; }
        public required string FirstName { get; set; } = null!;
        public required string LastName { get; set; } = null!;
        public required string DNI { get; set; } = null!;
        public required string Email { get; set; } = null!;
        public required string UserName { get; set; } = null!;
        public required string Password { get; set; } = null!;
        public required string ConfirmPassword { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? PhotoUrl { get; set; }
        public required string Role { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
