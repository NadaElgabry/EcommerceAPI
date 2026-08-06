namespace EcommerceAPI.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int? ParentId { get; set; }
        public Category? ParentCategory { get; set; }

        public List<Category> SubCategories { get; set; } = new List<Category>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
