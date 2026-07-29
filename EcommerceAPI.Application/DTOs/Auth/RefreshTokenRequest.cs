using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.Auth
{
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
