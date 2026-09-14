using EcommerceAPI.Application.DTOs.ProductReview;
using FluentValidation;

namespace EcommerceAPI.Application.Validators.ProductReview
{
    public class GetProductReviewsRequestValidator
        : AbstractValidator<GetProductReviewsRequest>
    {
        public GetProductReviewsRequestValidator()
        {
            RuleFor(request => request.Limit)
                .InclusiveBetween(1, 100)
                .WithMessage("Limit must be between 1 and 100.");
        }
    }
}
