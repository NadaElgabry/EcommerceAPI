using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EcommerceAPI.Application.Interfaces.VisualSearch;


namespace EcommerceAPI.Infrastructure.Services.VisualSearch
{
    public class VisualSearchService : IVisualSearchService
    {
        private readonly HttpClient _httpClient;

        public VisualSearchService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<string>> SearchByImageAsync(IFormFile image, int topK, CancellationToken ct)
        {
            using var content = new MultipartFormDataContent();
            using var stream = image.OpenReadStream();
            content.Add(new StreamContent(stream), "file", image.FileName);

            var response = await _httpClient.PostAsync($"/search?top_k={topK}", content, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<VisualSearchApiResponse>(cancellationToken: ct);
            return result?.Products ?? [];
        }

        private sealed class VisualSearchApiResponse
        {
            [JsonPropertyName("top_k")]
            public int TopK { get; set; }

            [JsonPropertyName("products")]
            public List<string> Products { get; set; } = [];
        }
    }
}
