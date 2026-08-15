using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.User;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Iservices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers([FromQuery] GetUsersRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.GetUsersAsync(request, cancellationToken);
            return Ok(ApiResponse<PagedResult<UserResponse>>.SuccessResponse(
                message: "Users retrieved successfully", statusCode: 200, data: result));
        }
    }
}
