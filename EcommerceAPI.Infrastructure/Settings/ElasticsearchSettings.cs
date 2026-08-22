using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Infrastructure.Settings
{
    public class ElasticsearchSettings
    {
        public string Url { get; set; } = string.Empty;
        public string ProductsIndex { get; set; } = "products";
    }
}
