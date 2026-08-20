using EcommerceAPI.Application.DTOs.Tag;
using FluentValidation;

namespace EcommerceAPI.Application.Validators.Tag
{
    public class CreateTagRequestValidator : AbstractValidator<CreateTagRequest>
    {
        public CreateTagRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(50);
        }
    }
}
