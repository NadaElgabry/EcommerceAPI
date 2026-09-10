using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EcommerceAPI.Application.Common;
namespace EcommerceAPI.Controllers
{
    [Route("api/chatbot")]
    [ApiController]
    public class RagController : ControllerBase
    {
        [HttpPost("send")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> send(
            [FromBody] string message,
            CancellationToken cancellationToken)
        {
            return Ok(ApiResponse<string>.SuccessResponse(message: "message received successfully", statusCode: 200, data: message));
        }
    }
}