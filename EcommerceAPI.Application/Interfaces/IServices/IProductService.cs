using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Product;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface IProductService
    {
        //public Task<CursorPagedResult<ProductListItemDto>> SearchProductsAsync(ProductQueryParams queryParams);
        public Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken);
        public Task<CursorPagedResponse<ProductSummaryResponse>> GetProductsPagedAsync(
            string? cursor,
            int pageSize,
            CancellationToken cancellationToken);
        public Task<ProductResponse> UpdateProductAsync(string slug,UpdateProductRequest request, CancellationToken cancellationToken);
        public Task DeleteProductAsync(string slug, CancellationToken cancellationToken);
    }
}
