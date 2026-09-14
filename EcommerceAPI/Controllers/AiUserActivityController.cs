using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.UserActivities;
using EcommerceAPI.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/ai/user-activities")]
    [ApiController]
    [Authorize(Policy = "ActivitiesRead")]
    public class AiUserActivityController : ControllerBase
    {
        private readonly IUserActivityService _userActivityService;

        public AiUserActivityController(
            IUserActivityService userActivityService)
        {
            _userActivityService = userActivityService;
        }

        [HttpGet]
        [ProducesResponseType(
            typeof(ApiResponse<CursorPagedResult<AiUserActivityResponse>>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetActivities(
            [FromQuery] Guid? userId,
            [FromQuery] string? cursor,
            [FromQuery] int pageSize,
            CancellationToken cancellationToken)
        {
            var result =
                await _userActivityService.GetActivitiesForAiAsync(
                    userId,
                    cursor,
                    pageSize,
                    cancellationToken);

            return Ok(
                ApiResponse<CursorPagedResult<AiUserActivityResponse>>
                    .SuccessResponse(
                        statusCode: 200,
                        message: "User activities retrieved successfully.",
                        data: result));
        }
    }
}