namespace EcommerceAPI.Domain.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public required string Slug { get; set; }
        public List<ProductTag> ProductTags { get; set; } = new();
    }
}