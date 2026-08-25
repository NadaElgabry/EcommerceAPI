using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Infrastructure.Settings
{
    public class ElasticsearchSettings
    {
        public string Url { get; set; } = string.Empty;
        public string ProductsIndex { get; set; } = "products";
        public string[] ProductSearchFields { get; set; } =
        { "name^3", "name.ngram^1", "brand^2", "tags^1.5", "description" };
    }
}
