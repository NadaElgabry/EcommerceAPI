using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.Search
{
    public class SearchFilter
    {
        public string Field { get; set; } = default!;
        public SearchFilterType Type { get; set; }
        public object? Value { get; set; }
        public IEnumerable<object>? Values { get; set; }
    }
}
