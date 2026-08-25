using EcommerceAPI.Application.DTOs.Auth;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Validators.Auth
{
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(request => request.NewPassword)
               .NotEmpty()
               .WithMessage("Password is required.")
               .MinimumLength(8)
               .WithMessage(
                   "Password must contain at least 8 characters."
               )
               .MaximumLength(100)
               .WithMessage(
                   "Password cannot exceed 100 characters."
               )
               .Matches(@"[0-9]")
               .WithMessage(
                   "Password must contain at least one number."
               )
               .Matches(@"[^a-zA-Z0-9]")
               .WithMessage(
                   "Password must contain at least one special character."
               );
        }
    }
}