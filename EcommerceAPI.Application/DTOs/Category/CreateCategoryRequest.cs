using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.Category
{
    public class CreateCategoryRequest
    {
        public required string Name { get; set; } = string.Empty;
        public required IFormFile Image { get; set; } 
    }
}
