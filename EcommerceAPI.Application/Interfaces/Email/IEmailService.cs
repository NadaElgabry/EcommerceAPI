using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
        
    }
}
