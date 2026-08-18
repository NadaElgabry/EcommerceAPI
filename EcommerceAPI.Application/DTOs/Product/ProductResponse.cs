using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.DTOs.Product
{
    public class ProductResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? ProductImageUrl { get; set; }
        public string? AltText { get; set; }
        public DateTime CreationDate { get; set; }
        public string CategoryName { get; set; }
        public List<string> Tags { get; set; } = new();
    }
}
