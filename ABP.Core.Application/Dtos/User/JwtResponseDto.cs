using System;
using System.Collections.Generic;

namespace ABP.Core.Application.Dtos.User
{
    public class JwtResponseDto
    {
        public string? Token { get; set; }
        public string? User { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime Expiration { get; set; }
        public bool? HasError { get; set; }
        public string? Error { get; set; }
    }
}
