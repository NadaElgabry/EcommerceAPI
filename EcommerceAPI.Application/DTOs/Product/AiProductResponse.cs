namespace EcommerceAPI.Application.DTOs.Product
{
    public class AiProductResponse
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string CategorySlug { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public List<string> Tags { get; set; } = new();
    }
}