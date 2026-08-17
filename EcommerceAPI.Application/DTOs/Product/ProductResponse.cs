namespace EcommerceAPI.Application.DTOs.Product
{
    public class ProductResponse
    {
        public string Slug { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ProductImage { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
