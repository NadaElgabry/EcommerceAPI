using System.Text.Json.Serialization;

namespace EcommerceAPI.Application.DTOs.Rag
{
    public class QuestionRequest
    {
        [JsonPropertyName("question")]
        public string Question { get; set; } = null!;

        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = null!;
    }
}