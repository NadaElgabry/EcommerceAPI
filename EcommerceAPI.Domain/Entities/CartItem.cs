using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Domain.Entities
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public Cart Cart { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Overwrites UnitPrice with the product's current price.
        /// Returns true if the price actually changed.
        /// </summary>
        public bool RefreshPrice(decimal currentPrice)
        {
            if (UnitPrice == currentPrice)
                return false;

            UnitPrice = currentPrice;
            return true;
        }
    }
}
