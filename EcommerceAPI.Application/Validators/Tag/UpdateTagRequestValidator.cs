using EcommerceAPI.Application.DTOs.Tag;
using FluentValidation;

namespace EcommerceAPI.Application.Validators.Tag
{
    public class UpdateTagRequestValidator : AbstractValidator<UpdateTagRequest>
    {
        public UpdateTagRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(50);
        }
    }
}
