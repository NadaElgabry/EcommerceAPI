using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Recommendation;
using EcommerceAPI.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/recommendations")]
    [ApiController]
    [Authorize]
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationController(
            IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRecommendations(
            [FromQuery] int limit = 10,
            CancellationToken cancellationToken = default)
        {
            var recommendations =
                await _recommendationService.GetRecommendationsAsync(
                    limit,
                    cancellationToken);

            return Ok(
                ApiResponse<List<RecommendedProductResponse>>.SuccessResponse(
                    statusCode: 200,
                    message: "Recommendations retrieved successfully.",
                    data: recommendations));
        }
    }
}