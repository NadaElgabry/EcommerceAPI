using EcommerceAPI.Application.DTOs.Recommendation;

namespace EcommerceAPI.Application.Interfaces.Recommendations
{
    public interface IRecommendationApiClient
    {
        Task<AiRecommendationResponse> GetRecommendationsAsync(
            int userId,
            int limit,
            CancellationToken cancellationToken);
    }
}
