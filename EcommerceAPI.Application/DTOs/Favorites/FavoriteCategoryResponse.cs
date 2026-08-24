namespace EcommerceAPI.Application.DTOs.Favorites
{
    public class FavoriteCategoryResponse
    {
        public required string Name { get; set; }
        public string Slug { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
