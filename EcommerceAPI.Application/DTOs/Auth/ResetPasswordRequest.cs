namespace EcommerceAPI.Application.DTOs.Auth
{
    public class ResetPasswordRequest
    {
        public required string ResetToken { get; set; }
        public required string NewPassword { get; set; }
    }
}
