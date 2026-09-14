using Microsoft.AspNetCore.Http;

namespace EcommerceAPI.Application.Interfaces.VisualSearch
{
    public interface IVisualSearchService
    {
        Task<List<string>> SearchByImageAsync(IFormFile image, int topK, CancellationToken cancellationToken);
    }
}
