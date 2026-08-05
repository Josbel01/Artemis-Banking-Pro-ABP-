using System;
using ABP.Core.Domain.Common;

namespace ABP.Core.Domain.Entities
{
    public class Commerce : BasicEntity<int>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Rnc { get; set; } = string.Empty;
        public string? UserId { get; set; } // Nullable, as the commerce can exist before assigning a user
    }
}
