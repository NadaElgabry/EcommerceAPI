namespace EcommerceAPI.Application.DTOs.Favorites
{
    public class FavoriteCategoryResponse
    {
        public int CategoryId { get; set; }
        public required string Name { get; set; }
        public string slug { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
