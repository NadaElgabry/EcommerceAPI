using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.UseCases.Auth;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RegisterUseCase _registerUseCase;

        public AuthController(RegisterUseCase registerUseCase)
        {
            _registerUseCase = registerUseCase;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            return Ok();
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(
            [FromBody] RegisterRequest request,
            CancellationToken cancellationToken)
        {
            AuthResponse response =
                await _registerUseCase.ExecuteAsync(
                    request,
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