using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Infrastructure.Settings
{
    public class RagSettings
    {
        public const string SectionName = "Rag";
        public string BaseUrl { get; set; } = null!;
        public int TimeoutSeconds { get; set; } = 60;
    }
}
