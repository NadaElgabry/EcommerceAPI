using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.Cart
{
    public class CartResponse
    {
        public decimal Total { get; set; }
        
        public List<CartItemResponse> Items { get; set; } = new List<CartItemResponse>();
    }
}
