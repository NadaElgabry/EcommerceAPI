using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Infrastructure.Settings
{
    public class EmailSettings
    {
        public const string SectionName = "EmailSettings";
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }      
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
    }
}
