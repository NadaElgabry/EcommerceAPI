using EcommerceAPI.Application.DTOs.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

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

    }
}
