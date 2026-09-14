using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.Cart
{
    public class UpdateCartRequest
    {
        public required string ProductSlug { get; set; }
        public decimal Quantity { get; set; }
    }
}
