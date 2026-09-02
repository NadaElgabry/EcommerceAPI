using EcommerceAPI.Application.DTOs.Rag;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Validators.Rag
{
    public class AskRequestValidator: AbstractValidator<AskRequest>
    {
        public AskRequestValidator()
        {
            RuleFor(request => request.Question)
                .NotEmpty()
                .WithMessage("Question cannot be empty.")
                .NotNull()
                .WithMessage("Question cannot be null")
                .MaximumLength(1000)
                .WithMessage("Question is too long.");

        }
    }
}
