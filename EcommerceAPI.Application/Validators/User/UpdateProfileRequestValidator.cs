using EcommerceAPI.Application.DTOs.Auth;
using FluentValidation;


namespace EcommerceAPI.Application.Validators
{
    public class UpdateProfileRequestValidator
        : AbstractValidator<UpdateProfileRequest>
    {
        public UpdateProfileRequestValidator()
        {
            RuleFor(request => request.FirstName)
                .MaximumLength(50)
                .WithMessage("First name cannot exceed 50 characters.")
                .When(request => request.FirstName != null);

            RuleFor(request => request.LastName)
                .MaximumLength(50)
                .WithMessage("Last name cannot exceed 50 characters.")
                .When(request => request.LastName != null);

            RuleFor(request => request.PhoneNumber)
                .MinimumLength(10)
                .WithMessage("Phone number must contain at least 10 characters.")
                .MaximumLength(15)
                .WithMessage("Phone number cannot exceed 15 characters.")
                .Matches(@"^(\+20|0020|0)?1[0125][0-9]{8}$")
                .WithMessage("Phone number must be a valid mobile number.")
                .When(request => request.PhoneNumber != null);
        }
    }
}
