using EcommerceAPI.Application.DTOs.Category;
using FluentValidation;

namespace EcommerceAPI.Application.Validators.Category
{
    public class CreateCategoryRequestValidator :AbstractValidator<CreateCategoryRequest>
    {
        public CreateCategoryRequestValidator() 
        {
            RuleFor(request => request.Name)
                .NotEmpty()
                .WithMessage("Category name is required.")
                .MaximumLength(100)
                .WithMessage("Category name must not exceed 100 characters.");
            RuleFor(request => request.Image)
                .NotEmpty()
                .WithMessage("Image is required.");
        }
    }
}
