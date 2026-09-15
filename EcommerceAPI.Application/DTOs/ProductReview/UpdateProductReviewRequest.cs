namespace EcommerceAPI.Application.DTOs.ProductReview
{
    public class UpdateProductReviewRequest
    {
        public int Rating { get; set; }

        public string? Comment { get; set; }
    }
}
