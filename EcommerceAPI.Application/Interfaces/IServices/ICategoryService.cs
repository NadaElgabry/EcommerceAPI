using EcommerceAPI.Application.DTOs.Category;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Product;

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

        public Task<CategoryResponse> GetCategoryDetailsAsync(
            string slug,
            CancellationToken cancellationToken);

        public Task<CursorPagedResult<ProductSummaryResponse>> GetCategoryProductsAsync(
            string slug,
            GetCategoriesRequest request,
            CancellationToken cancellationToken);

        public Task<CategoryResponse> UpdateCategoryAsync(
            string slug,
            UpdateCategoryRequest request,
            CancellationToken cancellationToken);

        public Task DeleteCategoryAsync(
            string slug,
            CancellationToken cancellationToken);
    }
}