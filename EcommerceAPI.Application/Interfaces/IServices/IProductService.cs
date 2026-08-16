using EcommerceAPI.Application.DTOs.Product;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface IProductService
    {
        Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken);
    }
}
