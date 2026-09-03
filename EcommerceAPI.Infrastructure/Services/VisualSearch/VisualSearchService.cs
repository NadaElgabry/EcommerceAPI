using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using EcommerceAPI.Application.Interfaces.VisualSearch;


namespace EcommerceAPI.Infrastructure.Services.VisualSearch
{
    public class VisualSearchService : IVisualSearchService
    {
        private readonly HttpClient _httpClient;

        public async Task<List<string>> SearchByImageAsync(IFormFile image, int topK, CancellationToken ct)
        {
            using var content = new MultipartFormDataContent();
            using var stream = image.OpenReadStream();
            content.Add(new StreamContent(stream), "file", image.FileName);

            var response = await _httpClient.PostAsync($"/search?top_k={topK}", content, ct);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<string>>(cancellationToken: ct) ?? [];
        }
    }
}
