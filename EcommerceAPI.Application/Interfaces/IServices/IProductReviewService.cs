using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.ProductReview;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface IProductReviewService
    {
        Task<ProductReviewResponse> CreateReviewAsync(
            string productSlug,
            CreateProductReviewRequest request,
            CancellationToken cancellationToken);

        Task<CursorPagedResult<ProductReviewResponse>> GetProductReviewsAsync(
            string productSlug,
            GetProductReviewsRequest request,
            CancellationToken cancellationToken);

        Task<List<AiProductReviewResponse>> GetReviewsForAiAsync(
            CancellationToken cancellationToken);

        Task<ProductReviewResponse> UpdateReviewAsync(
            string productSlug,
            int reviewId,
            UpdateProductReviewRequest request,
            CancellationToken cancellationToken);

        Task DeleteReviewAsync(
            string productSlug,
            int reviewId,
            CancellationToken cancellationToken);
    }
}
