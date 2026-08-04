using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.UseCases.Auth.Login;
using Microsoft.AspNetCore.Http;
using EcommerceAPI.Application.UseCases.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using EcommerceAPI.Application.Interfaces.Iservices;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILoginUseCase _loginUseCase;
        private readonly IAuthService _authService;

        public AuthController(ILoginUseCase loginUseCase, IAuthService authService)
        {
            _loginUseCase = loginUseCase;
            _authService = authService;
        } 

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var deviceInfo = Request.Headers["User-Agent"].ToString();
            var result = await _loginUseCase.Login(request, ipAddress, deviceInfo, cancellationToken);
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(
            [FromBody] RegisterRequest request,
            CancellationToken cancellationToken)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var deviceInfo = Request.Headers["User-Agent"].ToString();
            AuthResponse response =
                await _authService.CreateUserAsync(
                    request,ipAddress,deviceInfo,
                    cancellationToken
                );

            return Ok(response);
        }

        [HttpPost("refresh")]
        public IActionResult Refresh(
            [FromBody] RefreshTokenRequest request)
        {
            return Ok();
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok();
        }
    }
}