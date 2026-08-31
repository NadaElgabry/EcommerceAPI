namespace EcommerceAPI.Application.DTOs.Category
{
    public class GetCategoriesRequest
    {
        public string? Cursor { get; set; }

        public int Limit { get; set; } = 20;
    }
}