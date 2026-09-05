using EcommerceAPI.Application.DTOs.ProductReview;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Mappers.Mappings
{
    public class ProductReviewMapper : IProductReviewMapper
    {
        public ProductReview ToEntity(
            CreateProductReviewRequest request,
            int userId,
            int productId)
        {
            return new ProductReview
            {
                UserId = userId,
                ProductId = productId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };
        }

        public ProductReviewResponse ToResponse(ProductReview review)
        {
            return new ProductReviewResponse
            {
                Id = review.Id,
                UserGuid = review.User.Guid,
                UserName = $"{review.User.FirstName} {review.User.LastName}".Trim(),
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt
            };
        }

        public void UpdateFromRequest(
            ProductReview review,
            UpdateProductReviewRequest request)
        {
            review.Rating = request.Rating;
            review.Comment = request.Comment;
            review.UpdatedAt = DateTime.UtcNow;
        }
    }
}
