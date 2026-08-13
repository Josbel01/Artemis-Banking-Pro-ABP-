using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABP.Infrastructure.Identity.Entities
{
    public class AppUser : IdentityUser
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Identification { get; set; }
        public string? ProfileImage { get; set; }
        public bool IsActive { get; set; } = false;
    }
}
