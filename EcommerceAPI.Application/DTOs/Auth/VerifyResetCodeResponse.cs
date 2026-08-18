using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.Auth
{
    public class VerifyResetCodeResponse
    {
        public required string ResetToken { get; set; }
    }
}
