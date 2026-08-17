using System.Collections.Generic;

namespace EcommerceAPI.Application.DTOs.Common
{
    public class CursorPagedResponse<T>
    {
        public List<T> Data { get; set; } = new();
        public string? NextCursor { get; set; }
        public bool HasNext { get; set; }
    }
}