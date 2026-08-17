namespace EcommerceAPI.Application.DTOs.Auth
{
    public class ResetPasswordRequest
    {
        public required string OldPassword { get; set; }

        public required string NewPassword { get; set; }

        public required string ConfirmNewPassword { get; set; }
    }
}