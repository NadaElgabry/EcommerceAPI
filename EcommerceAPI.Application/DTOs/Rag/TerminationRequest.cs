using System.Text.Json.Serialization;

namespace EcommerceAPI.Application.DTOs.Rag
{
    public class TerminationRequest
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = null!;
    }
}