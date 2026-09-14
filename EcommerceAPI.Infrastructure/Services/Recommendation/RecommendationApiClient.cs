using System.Net.Http.Json;
using EcommerceAPI.Application.DTOs.Recommendation;
using EcommerceAPI.Application.Interfaces.Recommendations;

namespace EcommerceAPI.Infrastructure.Services.Recommendation
{
    public class RecommendationApiClient : IRecommendationApiClient
    {
        private readonly HttpClient _httpClient;

        public RecommendationApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AiRecommendationResponse> GetRecommendationsAsync(
            int userId,
            int limit,
            CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync(
                $"/v1/users/{userId}/recommendations?limit={limit}",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var recommendations =
                await response.Content.ReadFromJsonAsync<AiRecommendationResponse>(
                    cancellationToken: cancellationToken);

            return recommendations
                ?? new AiRecommendationResponse();
        }
    }
}
