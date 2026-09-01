using EcommerceAPI.Application.DTOs.ServiceAuth;
using FluentValidation;

namespace EcommerceAPI.Application.Validators.ServiceAuth
{
    public class ServiceTokenRequestValidator : AbstractValidator<ServiceTokenRequest>
    {
        public ServiceTokenRequestValidator()
        {
            RuleFor(x => x.ClientId).NotEmpty();
            RuleFor(x => x.ClientSecret).NotEmpty();
        }
    }
}