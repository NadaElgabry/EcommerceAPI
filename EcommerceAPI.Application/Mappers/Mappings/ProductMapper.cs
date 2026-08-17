using EcommerceAPI.Application.DTOs.Product;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Mappers.Mappings
{
    public class ProductMapper : IProductMapper
    {
        public Product ToProduct(CreateProductRequest request ,string slug, string imageUrl) 
        {
            return new Product
            {
                Name = request.Name,
                Slug = slug,
                Description = request.Description,
                Brand = request.Brand,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                AltText = request.AltText,
                CategoryId = request.CategoryId,
                ProductImage = imageUrl,
                CreationDate = DateTime.UtcNow
            };
        }

        public ProductResponse ToProductResponse(Product product)
        {
            return new ProductResponse
            {
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                Brand = product.Brand,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                AltText = product.AltText,
                CategoryId = product.CategoryId,
                ProductImage = product.ProductImage,
                CreationDate = DateTime.UtcNow
            };
        }
        public ProductSummaryResponse ToProductSummaryResponse(Product product)
        {
            return new ProductSummaryResponse
            {
                Name = product.Name,
                Slug = product.Slug,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                AltText = product.AltText,
                ProductImage = product.ProductImage,
            };
        }

        public void UpdateProductFromRequest(Product product, UpdateProductRequest request)
        {
            product.Description = request.Description;
            product.Brand = request.Brand;
            product.Price = request.Price;
            product.StockQuantity = request.StockQuantity;
            product.AltText = request.AltText;
        }



    }
}
