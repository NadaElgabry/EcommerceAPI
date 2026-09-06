using EcommerceAPI.Application.DTOs.ProductReview;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Mappers.Interfaces
{
    public interface IProductReviewMapper
    {
        ProductReview ToEntity(
            CreateProductReviewRequest request,
            int userId,
            int productId);

        ProductReviewResponse ToResponse(ProductReview review);

        void UpdateFromRequest(
            ProductReview review,
            UpdateProductReviewRequest request);
    }
}
