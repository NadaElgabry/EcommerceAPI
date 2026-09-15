namespace EcommerceAPI.Application.DTOs.ProductReview
{
    public class AiProductReviewResponse
    {
        public int ReviewId { get; set; }

        public int UserId { get; set; }

        public int ProductId { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
