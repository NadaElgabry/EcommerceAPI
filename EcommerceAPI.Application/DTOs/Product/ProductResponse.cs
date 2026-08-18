using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.DTOs.Product
{
    public class ProductResponse
    {
        public string Slug { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string brand {  get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ProductImageUrl { get; set; }

        public string CategoryName { get; set; }

        public List<Tag> Tags { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
