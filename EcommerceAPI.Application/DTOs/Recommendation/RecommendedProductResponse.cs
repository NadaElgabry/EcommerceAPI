using EcommerceAPI.Application.DTOs.Product;

namespace EcommerceAPI.Application.DTOs.Recommendation
{
    public class RecommendedProductResponse
    {
        public int Rank { get; set; }

        public double Score { get; set; }

        public string Source { get; set; } = string.Empty;

        public ProductSummaryResponse Product { get; set; } = null!;
    }
}