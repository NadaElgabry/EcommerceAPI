using EcommerceAPI.Domain.Enums;
using FluentValidation;

namespace EcommerceAPI.Application.Validators.Order
{
    public class GetAllOrdersRequestValidator : AbstractValidator<GetAllOrdersRequest>
    {
        public GetAllOrdersRequestValidator()
        {
            RuleFor(request => request.Status)
                .Must(status => string.IsNullOrEmpty(status) || Enum.TryParse<OrderStatus>(status, ignoreCase: true, out _))
                .WithMessage($"Status must be one of the following: {string.Join(", ", Enum.GetNames<OrderStatus>())}.");
        }
    }
}