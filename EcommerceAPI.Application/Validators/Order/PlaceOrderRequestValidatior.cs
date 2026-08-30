using EcommerceAPI.Application.DTOs.Order;
using FluentValidation;

namespace EcommerceAPI.Application.Validators.Order
{
    public class PlaceOrderRequestValidator : AbstractValidator<PlaceOrderRequest>
    {
        public PlaceOrderRequestValidator()
        {
            RuleFor(request => request.Address)
                .NotEmpty()
                .WithMessage("Address cannot be null or empty.");
        }
    }
}