using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Application.DTOs.Auth
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Old password is required.")]
        public required string OldPassword { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(
            8,
            ErrorMessage = "New password must be at least 8 characters long."
        )]
        [RegularExpression(
            @"^(?=.*\d)(?=.*[^a-zA-Z0-9\s]).+$",
            ErrorMessage = "New password must contain at least one number and one special character."
        )]
        public required string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm new password is required.")]
        [Compare(
            nameof(NewPassword),
            ErrorMessage = "New password and confirmation password must match."
        )]
        public required string ConfirmNewPassword { get; set; }
    }
}