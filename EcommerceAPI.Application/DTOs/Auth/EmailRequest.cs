using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.Auth
{
    public class EmailRequest
    {
        public required string Email { get; set; } = string.Empty;
    }
}
