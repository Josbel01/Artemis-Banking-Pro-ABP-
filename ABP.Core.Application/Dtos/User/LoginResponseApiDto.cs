using System.Collections.Generic;

namespace ABP.Core.Application.Dtos.User
{
    public class LoginResponseApiDto
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public string UserName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public bool HasError { get; set; }
        public required List<string> Errors { get; set; }
        public string? AccessToken { get; set; }
        public DateTime Expiration { get; set; }
    }
}
