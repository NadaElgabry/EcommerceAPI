using EcommerceAPI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
        public required string IdempotencyKey { get; set; }
        public required string Address { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime DeliveryTime { get; set; }
        
        public List<OrderItem> Items { get; set; } = new List<OrderItem>(); 

    }
}
