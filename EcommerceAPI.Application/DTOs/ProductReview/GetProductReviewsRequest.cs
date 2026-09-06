namespace EcommerceAPI.Application.DTOs.ProductReview
{
    public class GetProductReviewsRequest
    {
        public string? Cursor { get; set; }

        public int Limit { get; set; } = 20;
    }
}
