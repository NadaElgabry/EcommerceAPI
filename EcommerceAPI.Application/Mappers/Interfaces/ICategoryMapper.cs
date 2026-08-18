using EcommerceAPI.Application.DTOs.Category;
using EcommerceAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Mappers.Interfaces
{
    public interface ICategoryMapper
    {
        public CategoryResponse toCategoryResponse(Category category);
    }
}
