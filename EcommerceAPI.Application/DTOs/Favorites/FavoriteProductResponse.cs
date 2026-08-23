namespace EcommerceAPI.Application.DTOs.Favorites
{
    public class FavoriteProductResponse
    {
        public int ProductId { get; set; }
        public required string Slug { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public string? ProductImage { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
