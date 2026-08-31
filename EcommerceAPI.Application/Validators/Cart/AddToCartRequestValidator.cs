using EcommerceAPI.Application.DTOs.Cart;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Validators.Cart
{
    public class AddToCartRequestValidator :AbstractValidator<AddToCartRequest>
    {
        public AddToCartRequestValidator() 
        {
            RuleFor(request => request.ProductSlug)
                .NotEmpty()
                .NotNull()
                .WithMessage("Product slug is required");

            RuleFor(request => request.Quantity)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Quantity must be a positive number");
                             
        }
    }
}
