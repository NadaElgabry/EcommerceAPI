using EcommerceAPI.Application.DTOs.Product;
using FluentValidation;

namespace EcommerceAPI.Application.Validators.Product
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(request => request.Description)
                .NotEmpty()
                .MaximumLength(2000);

            RuleFor(request => request.Brand)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(request => request.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0.");

            RuleFor(request => request.SalePrice)
                .GreaterThan(0)
                .When(request => request.SalePrice.HasValue)
                .WithMessage("Sale price must be greater than 0.");

            RuleFor(request => request.SalePrice)
                .LessThan(request => request.Price)
                .When(request => request.SalePrice.HasValue)
                .WithMessage("Sale price must be less than the original price.");

            RuleFor(request => request.DiscountPercentage)
                .InclusiveBetween(0, 100)
                .When(request => request.DiscountPercentage.HasValue)
                .WithMessage("Discount percentage must be between 0 and 100.");

            RuleFor(request => request.StockQuantity)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Stock quantity cannot be negative.");

            RuleFor(request => request.AltText)
                .MaximumLength(255)
                .When(request => !string.IsNullOrWhiteSpace(request.AltText));

            RuleFor(request => request.Image)
                .NotEmpty()
                .WithMessage("Image is required.");
        }
    }
}