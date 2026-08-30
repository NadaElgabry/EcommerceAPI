using EcommerceAPI.Application.DTOs.Category;
using FluentValidation;

namespace EcommerceAPI.Application.Validators.Category
{
    public class GetCategoriesRequestValidator
        : AbstractValidator<GetCategoriesRequest>
    {
        public GetCategoriesRequestValidator()
        {
            RuleFor(request => request.Limit)
                .InclusiveBetween(1, 100)
                .WithMessage("Limit must be between 1 and 100.");
        }
    }
}