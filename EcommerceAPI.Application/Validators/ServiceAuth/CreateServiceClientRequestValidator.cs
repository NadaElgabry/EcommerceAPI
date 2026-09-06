using EcommerceAPI.Application.DTOs.ServiceAuth;
using FluentValidation;

namespace EcommerceAPI.Application.Validators.ServiceAuth
{
    public class CreateServiceClientRequestValidator : AbstractValidator<CreateServiceClientRequest>
    {
        public CreateServiceClientRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Scopes).NotEmpty().Must(s => s.All(scope => !string.IsNullOrWhiteSpace(scope)));
        }
    }
}