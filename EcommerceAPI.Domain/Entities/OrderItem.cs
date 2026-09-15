using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public Product? Product { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public string ProductName { get; set; } = default!;
        public string ProductSlug { get; set; } = default!;
        public string ProductDescription { get; set; } = default!;
        public string? ProductImageUrl { get; set; }
        public string? ProductAltText { get; set; }
    }
}
