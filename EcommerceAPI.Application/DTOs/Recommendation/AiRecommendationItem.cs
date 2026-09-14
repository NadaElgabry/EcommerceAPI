using System.Text.Json.Serialization;

namespace EcommerceAPI.Application.DTOs.Recommendation
{
    public class AiRecommendationItem
    {
        [JsonPropertyName("product_id")]
        public int ProductId { get; set; }

        [JsonPropertyName("rank")]
        public int Rank { get; set; }

        [JsonPropertyName("score")]
        public double Score { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;
    }
}