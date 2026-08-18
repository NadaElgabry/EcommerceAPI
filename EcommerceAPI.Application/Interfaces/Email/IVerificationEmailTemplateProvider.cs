using EcommerceAPI.Domain.Enums;

namespace EcommerceAPI.Application.Interfaces.Email
{
    public interface IVerificationEmailTemplateProvider
    {
        (string Subject, string Body) GetTemplate(VerificationPurpose purpose, string rawToken);
    }
}