using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.UseCases.Auth.Login;
using EcommerceAPI.Application.UseCases.Auth.Logout;
using EcommerceAPI.Application.UseCases.Auth.Refresh;
using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Application.Interfaces.IServices;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IRefreshUseCase _refreshUseCase;
        private readonly ILogoutUseCase _logoutUseCase;
        private readonly ILoginUseCase _loginUseCase;
        private readonly IAuthService _authService;

        public AuthController(ILoginUseCase loginUseCase, IAuthService authService, IRefreshUseCase refreshUseCase, ILogoutUseCase logoutUseCase)
        {
            _refreshUseCase = refreshUseCase;
            _logoutUseCase = logoutUseCase;
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
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        { 
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var deviceInfo = Request.Headers["User-Agent"].ToString();
            var response = await _refreshUseCase.Refresh(request, ipAddress, deviceInfo, cancellationToken);
            return Ok(response);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken) 
        { 
            await _logoutUseCase.Logout(request, cancellationToken);
            return NoContent();        
        }
    }
}