using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.Common
{
    public class CursorPageInfo
    {
        public string? NextCursor { get; set; }
        public bool HasNext { get; set; }
        public int PageSize { get; set; }
    }
}
