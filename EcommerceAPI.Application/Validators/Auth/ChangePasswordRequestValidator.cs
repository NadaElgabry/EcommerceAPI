using EcommerceAPI.Application.DTOs.Auth;
using FluentValidation;

namespace EcommerceAPI.Application.Validators.Auth
{
    public class ChangePasswordRequestValidator
        : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(request => request.OldPassword)
                .NotEmpty()
                .WithMessage("Old password is required.");

            RuleFor(request => request.NewPassword)
                .NotEmpty()
                .WithMessage("New password is required.")
                .MinimumLength(8)
                .WithMessage("New password must be at least 8 characters long.")
                .Matches(@"\d")
                .WithMessage("New password must contain at least one number.")
                .Matches(@"[^a-zA-Z0-9\s]")
                .WithMessage("New password must contain at least one special character.");

            RuleFor(request => request.ConfirmNewPassword)
                .NotEmpty()
                .WithMessage("Confirm new password is required.")
                .Equal(request => request.NewPassword)
                .WithMessage("New password and confirmation password must match.");
        }
    }
}
