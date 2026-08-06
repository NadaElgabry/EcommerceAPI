using EcommerceAPI.Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Application.Interfaces.IServices;

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
            return Ok(result);
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

            return Ok(token);
        }

        [HttpPost("ActivateAccount")]
        public async Task<IActionResult> ActivateAccount([FromBody] ActivateEmailRequest request, CancellationToken cancellationToken)
        {
            var response = await _authService.ActivateEmailAsync(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("IsEmailAvailable")]
        public async Task<IActionResult> IsEmailAvailable([FromBody] EmailRequest request, CancellationToken cancellationToken)
        {
            var isAvailable = await _authService.IsEmailAvailable(request, cancellationToken);
            return Ok(isAvailable);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        { 
            var response = await _authService.Refresh(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken) 
        { 
            await _authService.Logout(request, cancellationToken);
            return NoContent();        
        }
    }
}