using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace EcommerceAPI.Application.DTOs.Product
{
    public class CreateProductRequest
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string AltText { get; set; }
        public string CategorySlug { get; set; }
        public IFormFile Image { get; set; }
        public List<int> TagIds { get; set; } = new();
    }
}