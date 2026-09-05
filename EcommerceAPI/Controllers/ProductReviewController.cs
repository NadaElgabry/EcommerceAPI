using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.ProductReview;
using EcommerceAPI.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/products/{productSlug}/reviews")]
    [ApiController]
    public class ProductReviewController : ControllerBase
    {
        private readonly IProductReviewService _productReviewService;

        public ProductReviewController(
            IProductReviewService productReviewService)
        {
            _productReviewService = productReviewService;
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(
            typeof(ApiResponse<ProductReviewResponse>),
            StatusCodes.Status201Created)]
        [ProducesResponseType(
            typeof(ApiResponse<string>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<string>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<string>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<string>),
            StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateReview(
            [FromRoute] string productSlug,
            [FromBody] CreateProductReviewRequest request,
            CancellationToken cancellationToken)
        {
            var review =
                await _productReviewService.CreateReviewAsync(
                    productSlug,
                    request,
                    cancellationToken);

            return Created(
                $"api/products/{productSlug}/reviews/{review.Id}",
                ApiResponse<ProductReviewResponse>.SuccessResponse(
                    statusCode: 201,
                    message: "Review created successfully.",
                    data: review));
        }

        [HttpGet]
        [ProducesResponseType(
            typeof(ApiResponse<List<ProductReviewResponse>>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<string>),
            StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductReviews(
            [FromRoute] string productSlug,
            CancellationToken cancellationToken)
        {
            var reviews =
                await _productReviewService.GetProductReviewsAsync(
                    productSlug,
                    cancellationToken);

            return Ok(
                ApiResponse<List<ProductReviewResponse>>.SuccessResponse(
                    statusCode: 200,
                    message: "Product reviews retrieved successfully.",
                    data: reviews));
        }

        [HttpPut("{reviewId:int}")]
        [Authorize]
        [ProducesResponseType(
            typeof(ApiResponse<ProductReviewResponse>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<string>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<string>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<string>),
            StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateReview(
            [FromRoute] string productSlug,
            [FromRoute] int reviewId,
            [FromBody] UpdateProductReviewRequest request,
            CancellationToken cancellationToken)
        {
            var review =
                await _productReviewService.UpdateReviewAsync(
                    productSlug,
                    reviewId,
                    request,
                    cancellationToken);

            return Ok(
                ApiResponse<ProductReviewResponse>.SuccessResponse(
                    statusCode: 200,
                    message: "Review updated successfully.",
                    data: review));
        }

        [HttpDelete("{reviewId:int}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(
            typeof(ApiResponse<string>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<string>),
            StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteReview(
            [FromRoute] string productSlug,
            [FromRoute] int reviewId,
            CancellationToken cancellationToken)
        {
            await _productReviewService.DeleteReviewAsync(
                productSlug,
                reviewId,
                cancellationToken);

            return NoContent();
        }
    }
}
