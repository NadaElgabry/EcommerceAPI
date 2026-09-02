using System.Text.Json.Serialization;

namespace EcommerceAPI.Application.DTOs.Rag
{
    public class AnswerResponse
    {
        [JsonPropertyName("question")]
        public string Question { get; set; } = null!;

        [JsonPropertyName("answer")]
        public string Answer { get; set; } = null!;
    }
}