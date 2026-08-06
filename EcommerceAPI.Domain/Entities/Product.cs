namespace EcommerceAPI.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; }
        public string Brand { get; set; }
        public string Ingrediants { get; set; }
        public string StockQuantity { get; set; }
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; }

    }
}
