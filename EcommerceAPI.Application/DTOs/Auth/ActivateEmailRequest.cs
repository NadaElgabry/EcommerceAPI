using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.Auth
{
    public class ActivateEmailRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}
