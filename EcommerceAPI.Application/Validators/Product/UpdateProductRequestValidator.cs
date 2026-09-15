using EcommerceAPI.Application.DTOs.Product;
using FluentValidation;

namespace EcommerceAPI.Application.Validators.Product
{
    public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
    {
        public UpdateProductRequestValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(request => request.Description)
                .NotEmpty()
                .MaximumLength(2000);

            RuleFor(request => request.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0.");

            RuleFor(request => request.StockQuantity)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Stock quantity cannot be negative.");
        }
    }
}
