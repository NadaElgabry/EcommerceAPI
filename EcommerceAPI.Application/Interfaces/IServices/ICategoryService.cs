using EcommerceAPI.Application.DTOs.Category;

namespace EcommerceAPI.Application.Interfaces.Iservices
{
    public interface ICategoryService
    {
        public Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken);
    }
}
