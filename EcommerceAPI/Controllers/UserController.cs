using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService usersService)
        {
            _userService = usersService;
        }

        [HttpPut("{userId}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(
            [FromRoute] Guid userId, [FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
        {
            await _userService.UpdateProfileAsync(
                userId,
                request,
                cancellationToken);

            return Ok(
                ApiResponse<string>.SuccessResponse(
                    message: "User updated successfully",
                    statusCode: 200));
        }
        [HttpGet("{userId}")]
        [Authorize]
        public async Task<IActionResult> Profile([FromRoute] Guid userId, CancellationToken cancellationToken)
        {
            var userProfile = await _userService.GetUserProfileAsync(userId, cancellationToken);
            return Ok(ApiResponse<UserResponse>.SuccessResponse(message: "User profile retrieved successfully", statusCode: 200, data: userProfile));

        }
    }
}
