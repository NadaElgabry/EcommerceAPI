using EcommerceAPI.Domain.Enums;

namespace EcommerceAPI.Application.Interfaces.Email
{
    public interface IVerificationEmailTemplateProvider
    {
        /// <summary>
        /// Gets the email template for the specified verification purpose and raw token.
        /// </summary>
        /// <param name="purpose">The purpose of the verification token.</param>
        /// <param name="rawToken">The raw verification token.</param>
        /// <returns>A tuple containing the email subject and body.</returns>
        (string Subject, string Body) GetTemplate(VerificationPurpose purpose, string rawToken);
    }
}