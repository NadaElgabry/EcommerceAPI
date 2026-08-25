using System.Text.Json.Serialization;

namespace EcommerceAPI.Infrastructure.Services.Search.Documents
{
    public class ProductSearchDocument
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;

        [JsonPropertyName("brand")]
        public string Brand { get; set; } = null!;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("stockQuantity")]
        public int StockQuantity { get; set; }

        [JsonPropertyName("productImage")]
        public string? ProductImage { get; set; }

        [JsonPropertyName("altText")]
        public string? AltText { get; set; }

        [JsonPropertyName("creationDate")]
        public DateTime CreationDate { get; set; }

        [JsonPropertyName("categorySlug")]
        public string CategorySlug { get; set; } = null!;

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();
    }
}
