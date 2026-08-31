namespace EcommerceAPI.Application.DTOs.Order
{
    public class OrderSummary
    {
        public Guid Guid { get; set; }
        public string OrderNumber { get; set; }
        public decimal Total { get; set; }
        public string Address { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public int TotalItems { get; set; }
        public string Status { get; set; }
    }
}