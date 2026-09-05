namespace EcommerceAPI.Application.DTOs.ProductReview
{
    public class CreateProductReviewRequest
    {
        public int Rating { get; set; }

        public string? Comment { get; set; }
    }
}
