using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.Cart
{
    public class AddToCartRequest
    {
        public required string ProductSlug { get; set; }
        public int Quantity { get; set; }
    }
}
