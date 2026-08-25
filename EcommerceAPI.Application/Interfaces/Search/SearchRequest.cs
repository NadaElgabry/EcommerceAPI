using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.Search
{
    public class SearchRequest
    {
        public List<SearchFilter> Filters { get; set; } = new();
        public string? SearchText { get; set; }
        public string[]? SearchFields { get; set; }
        public string? SortField { get; set; }
        public SearchSortDirection SortDir { get; set; } = SearchSortDirection.Desc;
        public int Limit { get; set; } = 20;
        public string? Cursor { get; set; }
    }
}
