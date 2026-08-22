using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.Common
{
    public class SearchCursorPayload
    {
        public string SortBy { get; set; } = null!;
        public string[] Values { get; set; } = Array.Empty<string>();
    }
}
