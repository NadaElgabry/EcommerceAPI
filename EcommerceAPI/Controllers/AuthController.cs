using EcommerceAPI.Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Common;

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

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request,
            CancellationToken cancellationToken)
        {
            var token = await _authService.CreateUserAsync(
                    request,
                    cancellationToken
                );

            return Created("Register",ApiResponse<string>.SuccessResponse(message: "User created successfully", statusCode: 201, data: token));
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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            var x= await _authService.ForgotPasswordAsync(request, cancellationToken);
            return Ok(new { message = $"If an account with that email exists, a reset code has been sent. {x}" });
        }

        [HttpPost("verify-reset-code")]
        public async Task<ActionResult<VerifyResetCodeResponse>> VerifyResetCode(
        [FromBody] VerifyResetCodeRequest request, CancellationToken cancellationToken)
        {
            var response = await _authService.VerifyResetCodeAsync(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            await _authService.ResetPasswordAsync(request, cancellationToken);
            return NoContent();
        }
    }
}