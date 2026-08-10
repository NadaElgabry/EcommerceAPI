using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Interfaces.IServices;
using IdempotentAPI.Filters;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController( IAuthService authService)
        {
            _authService = authService;
        } 


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.Login(request, cancellationToken);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(message: "Login successful", statusCode: 200, data: result));
        }

        [Idempotent(ExpiresInMilliseconds = 10 * 1000)]
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request,
            CancellationToken cancellationToken)
        {
            await _authService.CreateUserAsync(
                    request,
                    cancellationToken
                );

            return Created("Register", ApiResponse<string>.SuccessResponse(message: "User created successfully", statusCode: 201));
        }

        [HttpPost("resend-email")]
        [Idempotent(ExpiresInMilliseconds = 10*1000)]
        public async Task<IActionResult> ResendEmail(
            [FromBody] ResendEmailRequest request,
            CancellationToken cancellationToken)
        {
            await _authService.ResendEmailAsync(request, cancellationToken);
            return Ok(ApiResponse<string>.SuccessResponse(message: "Email verification code resent successfully", statusCode: 200));
        }

        [HttpPost("ActivateAccount")]
        public async Task<IActionResult> ActivateAccount([FromBody] ActivateEmailRequest request, CancellationToken cancellationToken)
        {
            var response = await _authService.ActivateEmailAsync(request, cancellationToken);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(message: "Email activated successfully", statusCode: 200, data: response));
        }

        [HttpPost("IsEmailAvailable")]
        public async Task<IActionResult> IsEmailAvailable([FromBody] EmailRequest request, CancellationToken cancellationToken)
        {
            var isAvailable = await _authService.IsEmailAvailable(request, cancellationToken);
            return Ok(ApiResponse<bool>.SuccessResponse(message: "Email availability checked", statusCode: 200, data: isAvailable));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        { 
            var response = await _authService.Refresh(request, cancellationToken);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(message: "Token refreshed successfully", statusCode: 200, data: response));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken) 
        { 
            await _authService.Logout(request, cancellationToken);
            return StatusCode(204, ApiResponse<string>.SuccessResponse(message: "Logged out successfully", statusCode: 204));        
        }
    }
}