using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.Auth
{
    public class LogoutRequest
    {
        public required string RefreshToken { get; init; }
    }
}
