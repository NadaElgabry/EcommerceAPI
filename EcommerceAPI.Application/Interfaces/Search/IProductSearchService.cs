using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Product;

namespace EcommerceAPI.Application.Interfaces.Search
{
    public interface IProductSearchService
    {
        /// <summary>
        /// Searches for products using the given query parameters.
        /// </summary>
        /// <param name="queryParams">The query parameters to use for searching.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>A cursor-paged result of product summaries.</returns>
        Task<CursorPagedResult<ProductSummaryResponse>> SearchProductsAsync(
            ProductQueryParamsRequest queryParams,
            CancellationToken cancellationToken = default);
    }
}