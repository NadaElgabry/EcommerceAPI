using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _UsersService;

        public UserController(IUserService usersService)
        {
            _UsersService = usersService;
        }

        [HttpPut("/{userId?}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(
            Guid? userId, [FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
        {
            await _UsersService.UpdateProfileAsync(
                userId,
                request,
                cancellationToken);

            return Ok(
                ApiResponse<string>.SuccessResponse(
                    message: "User updated successfully",
                    statusCode: 200));
        }
    }
}
