using EcommerceAPI.Application.Interfaces.Email;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Infrastructure.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Diagnostics;

namespace EcommerceAPI.Infrastructure.Services.Mail
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }
        public async Task SendEmailAsync(
            string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;
                email.Body = new TextPart("html") { Text = htmlBody };

                using var smtp = new SmtpClient{
                    Timeout = 5000
                };

                await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, cancellationToken);
                await smtp.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
                await smtp.SendAsync(email, cancellationToken);
                await smtp.DisconnectAsync(true, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send email | To: {Recipient} | Subject: {Subject} | SmtpHost: {SmtpHost}:{SmtpPort} | Type: {ExceptionType}",
                    to, subject, _settings.Host, _settings.Port, ex.GetType().Name);
                
            }

        }
    }
}
