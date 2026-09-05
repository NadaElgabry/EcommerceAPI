using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.ProductReview;
using EcommerceAPI.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/reviews")]
    [ApiController]
    [Authorize(Policy = "ReviewsRead")]
    public class AiProductReviewController : ControllerBase
    {
        private readonly IProductReviewService _productReviewService;

        public AiProductReviewController(
            IProductReviewService productReviewService)
        {
            _productReviewService = productReviewService;
        }

        [HttpGet]
        [ProducesResponseType(
            typeof(ApiResponse<List<AiProductReviewResponse>>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetReviews(
            CancellationToken cancellationToken)
        {
            var reviews =
                await _productReviewService.GetReviewsForAiAsync(
                    cancellationToken);

            return Ok(
                ApiResponse<List<AiProductReviewResponse>>
                    .SuccessResponse(
                        statusCode: 200,
                        message: "Reviews retrieved successfully.",
                        data: reviews));
        }
    }
}
