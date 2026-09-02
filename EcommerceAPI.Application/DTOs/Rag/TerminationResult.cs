using EcommerceAPI.Application.DTOs.Product;

namespace EcommerceAPI.Application.DTOs.Rag
{
    public class TerminationResult
    {
        public string UserId { get; set; } = null!;
        public List<ProductSummaryResponse> SuggestedProducts { get; set; } = new();
    }
}