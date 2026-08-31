namespace EcommerceAPI.Application.DTOs.Order
{
    public class OrderItemResponse
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public int Quantity { get; set; }
        public string Brand { get; set; }
        public decimal UnitPrice { get; set; }
        public string? ProductImage { get; set; }
        public string? AltText { get; set; }
        public string Description { get; set; }
    }
}