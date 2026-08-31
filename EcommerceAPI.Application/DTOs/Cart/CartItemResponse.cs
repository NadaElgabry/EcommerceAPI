using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.Cart
{
    public class CartItemResponse
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public string ProductImageUrl { get; set; }
        public string AltText { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public bool PriceChanged { get; set; }
    }
}
