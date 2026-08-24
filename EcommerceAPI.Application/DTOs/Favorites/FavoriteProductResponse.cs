namespace EcommerceAPI.Application.DTOs.Favorites
{
    public class FavoriteProductResponse
    {
        public required string Slug { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public string? ProductImageUrl { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
