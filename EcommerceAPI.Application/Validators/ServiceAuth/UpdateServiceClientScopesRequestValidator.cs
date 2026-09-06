using EcommerceAPI.Application.DTOs.ServiceAuth;
using FluentValidation;

namespace EcommerceAPI.Application.Validators.ServiceAuth
{
    public class UpdateServiceClientScopesRequestValidator : AbstractValidator<UpdateServiceClientScopesRequest>
    {
        public UpdateServiceClientScopesRequestValidator()
        {
            RuleFor(x => x.Scopes).NotEmpty().Must(s => s.All(scope => !string.IsNullOrWhiteSpace(scope)));

            // ScopesCsv is nvarchar(500) — reject before EF hits a truncation error
            RuleFor(x => x.Scopes)
                .Must(s => string.Join(',', s).Length <= 500)
                .WithMessage("The combined scopes exceed the maximum length of 500 characters.");
        }
    }
}