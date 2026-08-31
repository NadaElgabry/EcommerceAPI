using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 0,
        Placed = 1,
        Shipped = 2,
        Delivered = 3,
        Cancelled = 4
    }
}
