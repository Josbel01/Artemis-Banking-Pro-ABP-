using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABP.Core.Application.Dtos.User
{
    public class UserDto
    {
        public required string Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? DNI { get; set; }
        public string? PhotoUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public List<string>? Roles { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
