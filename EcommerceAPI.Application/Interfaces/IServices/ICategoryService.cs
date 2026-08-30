using EcommerceAPI.Application.DTOs.Category;
using EcommerceAPI.Application.DTOs.Common;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface ICategoryService
    {
        public Task<CategoryResponse> CreateCategoryAsync(
            CreateCategoryRequest request,
            CancellationToken cancellationToken);

        public Task<CursorPagedResult<CategoryResponse>> GetCategoriesAsync(
            GetCategoriesRequest request,
            CancellationToken cancellationToken);

        public Task DeleteCategoryAsync(
            string slug,
            CancellationToken cancellationToken);
    }
}