using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces.Iservices;
using EcommerceAPI.Application.UseCases.Auth.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILoginUseCase _loginUseCase;
        private readonly IAuthService _authService;

        public AuthController(
            ILoginUseCase loginUseCase,
            IAuthService authService)
        {
            _loginUseCase = loginUseCase;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken)
        {
            var ipAddress =
                HttpContext.Connection.RemoteIpAddress?.ToString()
                ?? "unknown";

            var deviceInfo =
                Request.Headers["User-Agent"].ToString();

            var result = await _loginUseCase.Login(
                request,
                ipAddress,
                deviceInfo,
                cancellationToken
            );

            return Ok(result);
        }

        [Authorize]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordRequest request,
            CancellationToken cancellationToken)
        {
            var subject =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (!Guid.TryParse(subject, out var userGuid))
            {
                throw new UnauthorizedException(
                    "Invalid authenticated user."
                );
            }

            await _authService.ResetPasswordAsync(
                userGuid,
                request,
                cancellationToken
            );

            return NoContent();
        }

        /*
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            return Ok();
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshTokenRequest request)
        {
            return Ok();
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok();
        }
        */
    }
}