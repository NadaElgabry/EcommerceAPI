namespace EcommerceAPI.Application.DTOs.Order
{
    public class OrderItemResponse
    {
        public string ProductSlug { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
