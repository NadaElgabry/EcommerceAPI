using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.Auth
{
    public class AuthResponse
    {
        public string? Token { get; set; }
        public Guid RefreshTokenId { get; set; }

    }
}
