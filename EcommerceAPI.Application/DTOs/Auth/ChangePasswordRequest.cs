using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.Auth
{
    public class ChangePasswordRequest
    {
        public required string OldPassword { get; set; }

        public required string NewPassword { get; set; }

        public required string ConfirmNewPassword { get; set; }

    }
}
