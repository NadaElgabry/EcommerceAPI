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
        public string? ProductImage { get; set; }
        public string? AltText { get; set; }
        public DateTime CreationDate { get; set; }
        public int CategoryId { get; set; }
    }
}
