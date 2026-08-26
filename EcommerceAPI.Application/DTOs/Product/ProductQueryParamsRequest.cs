using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.Product
{
    public class ProductQueryParamsRequest
    {
        public string? CategorySlug { get; set; }
        public List<string>? Tags { get; set; }
        public List<string>? Brand { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? InStock { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string? SortDir { get; set; } = "desc";
        public string? Cursor { get; set; }
        public int Limit { get; set; } = 20;
    }
}
