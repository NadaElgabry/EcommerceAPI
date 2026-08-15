using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.Common
{
    public class PagedResult<T>
    {
        public required List<T> Items { get; set; }

        public string? StartCursor { get; set; }
        public string? EndCursor { get; set; }

        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
