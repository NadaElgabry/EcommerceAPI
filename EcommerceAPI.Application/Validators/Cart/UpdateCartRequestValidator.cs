using EcommerceAPI.Application.DTOs.Cart;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Validators.Cart
{
    public class UpdateCartRequestValidator :AbstractValidator<UpdateCartRequest>
    { 
        public UpdateCartRequestValidator() 
        {
            RuleFor(request => request.ProductSlug)
                .NotEmpty()
                .NotNull()
                .WithMessage("Product slug is required");

            RuleFor(request => request.Quantity)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Quantity cannot be negative");;
        }
    }
}
