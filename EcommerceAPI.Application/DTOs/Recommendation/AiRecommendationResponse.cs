using System.Text.Json.Serialization;

namespace EcommerceAPI.Application.DTOs.Recommendation
{
    public class AiRecommendationResponse
    {
        [JsonPropertyName("items")]
        public List<AiRecommendationItem> Items { get; set; } = new();
    }
}