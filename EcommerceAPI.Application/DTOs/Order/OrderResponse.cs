using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.Order
{
    public class OrderResponse
    {
        public Guid Guid { get; set; }
        public string OrderNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Address { get; set; } = string.Empty;
        public DateTime DeliveryTime { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new();
    }
}
