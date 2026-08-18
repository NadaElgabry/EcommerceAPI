using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }

        public required string Name { get; set; } = string.Empty;

        public required string ImageUrl { get; set; } = string.Empty;

        public required string Slug {get; set; } = string.Empty;

        public List<Product> Products{ get; set; } = new();
    
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
