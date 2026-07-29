using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.UseCases.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RefreshUseCase _refreshUseCase;

        public AuthController(RefreshUseCase refreshUseCase)
        {
            _refreshUseCase = refreshUseCase;
        }


        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Implementation for login logic
            return Ok();
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
        public IActionResult Logout()
        {
            // Implementation for logout logic
            return Ok();
        }

    }
}
