namespace EcommerceAPI.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }

        public string Slug { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public string ProductImage { get; set; }

        public string AltText { get; set; }

        public DateTime CreationDate { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public List<ProductTag> ProductTags { get; set; }
            = new List<ProductTag>();
    }
}
