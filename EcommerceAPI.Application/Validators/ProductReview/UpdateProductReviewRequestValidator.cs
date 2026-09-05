using EcommerceAPI.Application.DTOs.ProductReview;
using FluentValidation;

namespace EcommerceAPI.Application.Validators.ProductReview
{
    public class UpdateProductReviewRequestValidator
        : AbstractValidator<UpdateProductReviewRequest>
    {
        public UpdateProductReviewRequestValidator()
        {
            RuleFor(request => request.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5.");
        }
    }
}
