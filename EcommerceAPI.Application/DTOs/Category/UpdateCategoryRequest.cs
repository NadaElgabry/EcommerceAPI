using Microsoft.AspNetCore.Http;

namespace EcommerceAPI.Application.DTOs.Category
{
    public class UpdateCategoryRequest
    {
        public string? Name { get; set; }

        public IFormFile? Image { get; set; }
    }
}