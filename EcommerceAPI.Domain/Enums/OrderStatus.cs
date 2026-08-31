using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Domain.Enums
{
    public enum OrderStatus
    {
        Placed = 0,
        Shipped = 1,
        Delivered = 2,
        Cancelled = 3
    }
}
