using EcommerceAPI.Application.DTOs.Recommendation;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface IRecommendationService
    {
        Task<List<RecommendedProductResponse>> GetRecommendationsAsync(
            int limit,
            CancellationToken cancellationToken);
    }
}