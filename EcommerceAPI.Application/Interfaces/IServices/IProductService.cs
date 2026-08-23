using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Product;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface IProductService
    {
        //public Task<CursorPagedResult<ProductListItemDto>> SearchProductsAsync(ProductQueryParams queryParams);
        public Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken);
        Task<ProductResponse> GetProductDetailsAsync(string slug, CancellationToken cancellationToken);
        public Task<ProductResponse> UpdateProductAsync(string slug,UpdateProductRequest request, CancellationToken cancellationToken);

        /// <summary>
        /// Searches products using the given query parameters.
        /// </summary>
        /// <param name="queryParams">The query parameters to use for searching.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>A cursor-paged result of product summaries.</returns>
        public Task<CursorPagedResult<ProductSummaryResponse>> SearchProductsAsync(
            ProductQueryParamsRequest queryParams, CancellationToken cancellationToken);

        public Task DeleteProductAsync(string slug, CancellationToken cancellationToken);
    }
}
