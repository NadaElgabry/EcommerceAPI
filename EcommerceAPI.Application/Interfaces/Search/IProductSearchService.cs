using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Product;

namespace EcommerceAPI.Application.Interfaces.Search
{
    public interface IProductSearchService
    {
        Task<CursorPagedResult<ProductSummaryResponse>> SearchProductsAsync(
            ProductQueryParamsRequest queryParams,
            CancellationToken cancellationToken = default);
    }
}