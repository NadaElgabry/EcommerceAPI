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
                .ValidPassword();

            RuleFor(request => request.ConfirmNewPassword)
                .NotEmpty()
                .WithMessage("Confirm new password is required.")
                .Equal(request => request.NewPassword)
                .WithMessage("New password and confirmation password must match.");
        }
    }
}
