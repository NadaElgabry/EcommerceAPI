using EcommerceAPI.Application.DTOs.Product;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Mappers.Mappings
{
    public class ProductMapper : IProductMapper
    {
        public Product ToProduct(CreateProductRequest request ,string slug, Category category, string imageUrl, List<Tag> validTags) 
        {
            var product = new Product
            {
                Name = request.Name,
                Slug = slug,
                Description = request.Description,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                AltText = request.AltText,
                CategoryId = category.Id,
                Category = category,
                ProductImage = imageUrl,
                CreationDate = DateTime.UtcNow
            };

            foreach (var tag in validTags)
            {
                product.ProductTags.Add(new ProductTag
                {
                    TagId = tag.Id,
                    Tag = tag
                });
            }

            return product;
        }

        public ProductResponse ToProductResponse(Product product)
        {
            return new ProductResponse
            {
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                AltText = product.AltText,
                CategoryName = product.Category.Name,
                ProductImageUrl = product.ProductImage,
                CreationDate = product.CreationDate,
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
                CategorySlug = product.Category.Slug,
                AltText = product.AltText,
                ProductImageUrl = product.ProductImage,
            };
        }

        public void UpdateProductFromRequest(Product product, UpdateProductRequest request)
        {
            product.Description = request.Description;
            product.Price = request.Price;
            product.StockQuantity = request.StockQuantity;
            product.AltText = request.AltText;
        }



    }
}
