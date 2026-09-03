using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.Search
{
    public class SearchRequest
    {
        public List<SearchFilter> Filters { get; set; } = new();
        public string? SearchText { get; set; }

        /// <summary>Fields for prefix / partial-character matching (edge-ngram subfields).</summary>
        public string[]? PrefixFields { get; set; }

        /// <summary>Fields for word-family matching (stemmed + synonym subfields).</summary>
        public string[]? SemanticFields { get; set; }

        /// <summary>Fields for exact/near-exact matching with typo tolerance (fuzziness).</summary>
        public string[]? ExactFields { get; set; }

        public string? SortField { get; set; }
        public SearchSortDirection SortDir { get; set; } = SearchSortDirection.Desc;
        public int Limit { get; set; } = 20;
        public string? Cursor { get; set; }
    }
}