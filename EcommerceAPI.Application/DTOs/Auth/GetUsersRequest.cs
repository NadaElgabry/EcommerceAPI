using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.User
{
    public class GetUsersRequest
    {
        public string? After { get; set; } 
        public string? Before { get; set; }

        public int PageSize { get; set; } = 20;
    }
}
