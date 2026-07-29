using EcommerceAPI.Application.DTOs.Auth;
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
        private readonly ILoginUseCase _loginUseCase;
        public AuthController(ILoginUseCase loginUseCase)
        {
            _loginUseCase = loginUseCase;
        } 

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var deviceInfo = Request.Headers["User-Agent"].ToString();
            var result = await _loginUseCase.Handle(request, ipAddress, deviceInfo);
            return Ok(result);
        }

        /*
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            // Implementation for register logic
            return Ok();
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshTokenRequest request)
        {
            // Implementation for refresh token logic
            return Ok();
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Implementation for logout logic
            return Ok();
        }
        */
    }
}
