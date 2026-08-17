using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.User
{
    public class GetUsersRequest
    {
        public int PageNumber { get; set; } = 0 ;
        public int PageSize { get; set; } = 10;
    }
}
