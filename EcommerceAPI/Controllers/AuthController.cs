using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.UseCases.Auth;
using EcommerceAPI.Application.UseCases.Auth.Login;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RefreshUseCase _refreshUseCase;
        private readonly LogoutUseCase _logoutUseCase;
        private readonly ILoginUseCase _loginUseCase;
        
        public AuthController(RefreshUseCase refreshUseCase, LogoutUseCase logoutUseCase)
        {
            _refreshUseCase = refreshUseCase;
            _logoutUseCase = logoutUseCase;
            _loginUseCase = loginUseCase;
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
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            // Implementation for register logic
            return Ok();
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var response = await _refreshUseCase.ExecuteAsync(request);
            return Ok(response);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            await _logoutUseCase.ExecuteAsync(request);
            return NoContent();
        }
    }
}
