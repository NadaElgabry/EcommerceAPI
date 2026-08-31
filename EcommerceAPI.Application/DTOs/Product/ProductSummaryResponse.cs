namespace EcommerceAPI.Application.DTOs.Product
{
    public class ProductSummaryResponse
    {
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public int StockQuantity { get; set; }
        public string? AltText { get; set; }
        public decimal Price { get; set; }
        public string? ProductImageUrl { get; set; }
    }
}
