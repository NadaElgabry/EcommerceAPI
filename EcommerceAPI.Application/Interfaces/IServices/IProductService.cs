using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Product;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface IProductService
    {
        public Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken);
        public Task<CursorPagedResponse<ProductResponse>> GetProductsPagedAsync(
            string? cursor,
            int pageSize,
            CancellationToken cancellationToken);
    }
}
