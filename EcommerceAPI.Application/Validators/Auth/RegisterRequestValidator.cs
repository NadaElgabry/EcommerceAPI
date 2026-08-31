using EcommerceAPI.Application.DTOs.Auth;
using FluentValidation;

namespace EcommerceAPI.Application.Validators.Auth
{
    public class RegisterRequestValidator
        : AbstractValidator<RegisterRequest>
    {
        private static readonly System.Text.RegularExpressions.Regex EgyptianPhoneRegex =
            new(@"^(\+20|0020|0)?1[0125][0-9]{8}$", System.Text.RegularExpressions.RegexOptions.Compiled);

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

            RuleFor(r => r.Email)
                    .NotEmpty().WithMessage("Email is required.")
                    .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.")
                    .EmailAddress().WithMessage("Email format is invalid.");

            RuleFor(r => r.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(EgyptianPhoneRegex)
                    .WithMessage("Phone number must be a valid Egyptian mobile number (e.g. 01012345678).");

            RuleFor(request => request.BirthDate)
                .NotEmpty()
                .WithMessage("Birth date is required.")
                .LessThan(DateTime.UtcNow.Date)
                .WithMessage("Birth Date is invalid");

            RuleFor(request => request.Password)
                .ValidPassword();
        }
    }
}