
namespace EcommerceAPI.Application.Interfaces.Email
{
    public interface IEmailService
    {
        
        /// <summary>
        /// Sends an email asynchronously.
        /// </summary>
        /// <param name="to">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="htmlBody">The HTML body of the email.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
        
    }
}
