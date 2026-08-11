using EcommerceAPI.Application.Interfaces.Email;
using EcommerceAPI.Domain.Enums;

namespace EcommerceAPI.Infrastructure.Services.Email
{
    public class VerificationEmailTemplateProvider : IVerificationEmailTemplateProvider
    {
        private static readonly Dictionary<VerificationPurpose, (string Subject, Func<string, string> Body)> Templates = new()
        {
            [VerificationPurpose.EmailVerification] = (
                "Activate your account",
                token => $"<p>Your activation code is: <strong>{token}</strong></p>"
            ),
            [VerificationPurpose.PasswordReset] = (
                "Reset your password",
                token => $"<p>Your password reset code is: <strong>{token}</strong></p>"
            ),
        };

        public (string Subject, string Body) GetTemplate(VerificationPurpose purpose, string rawToken)
        {
            if (!Templates.TryGetValue(purpose, out var template))
                throw new InvalidOperationException($"No email template configured for purpose '{purpose}'.");

            return (template.Subject, template.Body(rawToken));
        }
    }
}