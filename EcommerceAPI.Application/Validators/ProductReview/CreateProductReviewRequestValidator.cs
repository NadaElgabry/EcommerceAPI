using EcommerceAPI.Application.DTOs.ProductReview;
using FluentValidation;

namespace EcommerceAPI.Application.Validators.ProductReview
{
    public class CreateProductReviewRequestValidator
        : AbstractValidator<CreateProductReviewRequest>
    {
        public CreateProductReviewRequestValidator()
        {
            RuleFor(request => request.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5.");
        }
    }
}
