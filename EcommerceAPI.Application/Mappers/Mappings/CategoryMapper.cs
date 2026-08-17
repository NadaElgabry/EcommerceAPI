using EcommerceAPI.Application.DTOs.Category;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Mappers.Mappings
{
    public class CategoryMapper : ICategoryMapper
    {
 
        public CategoryResponse toCategoryResponse(Category category)
        {
            return new CategoryResponse
            {
                Name = category.Name,
                ImageUrl = category.ImageUrl,
                CreatedAt = category.CreatedAt
            };
        }
    }
}
