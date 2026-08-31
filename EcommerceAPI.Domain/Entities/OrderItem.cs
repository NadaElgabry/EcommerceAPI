using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product product { get; set; }
        public int OrderId { get; set; }
        public Order order { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
