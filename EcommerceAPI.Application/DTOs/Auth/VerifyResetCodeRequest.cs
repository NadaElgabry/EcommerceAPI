using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.Auth
{
    public class VerifyResetCodeRequest
    {
        public required string Code { get; set; }

    }
}
