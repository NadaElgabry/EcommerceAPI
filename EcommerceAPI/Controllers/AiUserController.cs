using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.User;
using EcommerceAPI.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/ai/users")]
    [ApiController]
    [Authorize(Policy = "AiUsersRead")]
    public class AiUserController : ControllerBase
    {
        private readonly IUserService _userService;

        public AiUserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [ProducesResponseType(
            typeof(ApiResponse<List<AiUserResponse>>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsers(
            CancellationToken cancellationToken)
        {
            var users =
                await _userService.GetUsersForAiAsync(
                    cancellationToken);

            return Ok(
                ApiResponse<List<AiUserResponse>>
                    .SuccessResponse(
                        statusCode: 200,
                        message: "Users retrieved successfully.",
                        data: users));
        }
    }
}
