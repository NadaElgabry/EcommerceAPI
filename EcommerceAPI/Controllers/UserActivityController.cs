using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Tag;
using EcommerceAPI.Application.DTOs.UserActivities;
using EcommerceAPI.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;

namespace EcommerceAPI.Controllers
{
    [Route("api/user-activities")]
    [ApiController]
    public class UserActivityController : ControllerBase
    {
        private readonly IUserActivityService _userActivityService;

        public UserActivityController(IUserActivityService userActivityService)
        {
            _userActivityService = userActivityService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserActivities([FromQuery] Guid? userId, [FromQuery] string? cursor, [FromQuery] int pageSize, CancellationToken cancellationToken)
        {
            var result = await _userActivityService.GetAllActivitiesAsync(userId, cursor, pageSize, cancellationToken);
            return Ok(ApiResponse<CursorPagedResult<UserActivitiesResponse>>.SuccessResponse(
                statusCode: 200, message: "User activities retrieved successfully.", data: result));
        }
    }
}
