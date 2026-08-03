using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.UseCases.Auth.Login;
using EcommerceAPI.Application.UseCases.Auth.Logout;
using EcommerceAPI.Application.UseCases.Auth.Refresh;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IRefreshUseCase _refreshUseCase;
        private readonly ILogoutUseCase _logoutUseCase;
        private readonly ILoginUseCase _loginUseCase;
        
        public AuthController(IRefreshUseCase refreshUseCase, ILogoutUseCase logoutUseCase , ILoginUseCase loginUseCase)
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
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var deviceInfo = Request.Headers["User-Agent"].ToString();
            var response = await _refreshUseCase.ExecuteAsync(request, ipAddress, deviceInfo, cancellationToken);
            return Ok(response);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
        {
            await _logoutUseCase.ExecuteAsync(request, cancellationToken);
            return NoContent();
        }
    }
}
