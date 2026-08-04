using EcommerceAPI.Application.DTOs.Auth;
using FluentValidation;

namespace EcommerceAPI.Application.Validators
{
    public class RegisterRequestValidator
        : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(request => request.FirstName)
                .NotEmpty()
                .WithMessage("First name is required.")
                .MaximumLength(50)
                .WithMessage(
                    "First name cannot exceed 50 characters."
                );

            RuleFor(request => request.LastName)
                .NotEmpty()
                .WithMessage("Last name is required.")
                .MaximumLength(50)
                .WithMessage(
                    "Last name cannot exceed 50 characters."
                );

            RuleFor(request => request.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Email format is invalid.")
                .MaximumLength(100)
                .WithMessage(
                    "Email cannot exceed 100 characters."
                );

            RuleFor(request => request.PhoneNumber)
                .NotEmpty()
                .WithMessage("Phone number is required.")
                .MinimumLength(7)
                .WithMessage(
                    "Phone number must contain at least 7 characters."
                )
                .MaximumLength(20)
                .WithMessage(
                    "Phone number cannot exceed 20 characters."
                )
                .Matches(@"^\+?[0-9\s\-()]+$")
                .WithMessage(
                    "Phone number contains invalid characters."
                );

            RuleFor(request => request.Password)
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