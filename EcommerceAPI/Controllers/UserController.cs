using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.User;
using EcommerceAPI.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

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
            var result = await _userService.UpdateProfileAsync(
                userId,
                request,
                cancellationToken);

            return Ok(
                ApiResponse<UserResponse>.SuccessResponse(
                    message: "User updated successfully",
                    statusCode: 200,
                    data:result));
        }

        [HttpGet("{userId}")]
        [Authorize(Policy = "UsersRead")]
        public async Task<IActionResult> Profile([FromRoute] Guid userId, CancellationToken cancellationToken)
        {
            var userProfile = await _userService.GetUserProfileAsync(userId, cancellationToken);
            return Ok(ApiResponse<UserResponse>.SuccessResponse(message: "User profile retrieved successfully", statusCode: 200, data: userProfile));

        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers([FromQuery] OffsetPageRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.GetAllUsersAsync(request, cancellationToken);
            return Ok(ApiResponse<OffsetPagedResult<UserResponse>>.SuccessResponse(
                message: "Users retrieved successfully", statusCode: 200, data: result));
        }
    }
}
