using EcommerceAPI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.Auth
{
    public class ResendEmailRequest
    {
        public required string Email { get; set; } = string.Empty;
        public required VerificationPurpose Purpose { get; set; } 

    }
}
