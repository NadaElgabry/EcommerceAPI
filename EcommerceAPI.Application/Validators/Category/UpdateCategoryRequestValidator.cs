using EcommerceAPI.Application.DTOs.Category;
using FluentValidation;

namespace EcommerceAPI.Application.Validators.Category
{
    public class UpdateCategoryRequestValidator
        : AbstractValidator<UpdateCategoryRequest>
    {
        public UpdateCategoryRequestValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty()
                .WithMessage("Category name cannot be empty.")
                .MaximumLength(100)
                .WithMessage("Category name must not exceed 100 characters.")
                .When(request => request.Name != null);

            RuleFor(request => request)
                .Must(request =>
                    request.Name != null ||
                    request.Image != null)
                .WithMessage(
                    "At least a category name or image must be provided.");
        }
    }
}