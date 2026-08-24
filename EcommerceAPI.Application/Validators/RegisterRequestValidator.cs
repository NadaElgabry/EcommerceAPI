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
                .MinimumLength(10)
                .WithMessage("Phone number must contain at least 10 characters.")
                .MaximumLength(15)
                .WithMessage("Phone number cannot exceed 15 characters.")
                .Matches(@"^(\+20|0020|0)?1[0125][0-9]{8}$")
                .WithMessage("Phone number must be a valid Egyptian mobile number (e.g. 01012345678).");

            RuleFor(request => request.BirthDate)
                .NotEmpty()
                .WithMessage("Birth date is required.")
                .LessThan(DateTime.UtcNow.Date)
                .WithMessage("Birth Date is invalid");

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