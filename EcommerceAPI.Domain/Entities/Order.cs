using EcommerceAPI.Domain.Enums;

namespace EcommerceAPI.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();

        public string OrderNumber { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public required string IdempotencyKey { get; set; }
        public required string Address { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime? DeliveryTime { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>(); 

    }
}
