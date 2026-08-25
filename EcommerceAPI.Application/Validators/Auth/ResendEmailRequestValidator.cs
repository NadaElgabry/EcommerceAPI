using EcommerceAPI.Application.DTOs.Auth;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Validators.Auth
{
    public class ResendEmailRequestValidator :AbstractValidator<ResendEmailRequest>
    {
        public ResendEmailRequestValidator() 
        {
            RuleFor(request => request.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Email format is invalid.")
                .MaximumLength(100)
                .WithMessage(
                    "Email cannot exceed 100 characters."
                );
        }

    }
}
