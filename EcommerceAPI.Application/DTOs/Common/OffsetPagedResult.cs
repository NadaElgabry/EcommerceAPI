using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.Common
{
    public class OffsetPagedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public PageInfo Pagination { get; set; } = new();
    }
}
