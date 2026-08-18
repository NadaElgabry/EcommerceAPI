using EcommerceAPI.Application.DTOs.Product;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Mappers.Interfaces
{
    public interface IProductMapper
    {
        Product ToProduct(CreateProductRequest request,string slug, string imageUrl);
        ProductResponse ToProductResponse(Product product);

    }
}
