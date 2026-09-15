using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Rag;
using EcommerceAPI.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RagController : ControllerBase
    {
        private readonly IRagService _ragService;

        public RagController(IRagService ragService)
        {
            _ragService = ragService;
        }

        [HttpPost("chat")]
        [Authorize]
        public async Task<ActionResult<AnswerResponse>> Ask(
            [FromBody] AskRequest request, CancellationToken cancellationToken)
        {
            var result = await _ragService.AskAsync(request.Question,cancellationToken);
            return Ok(ApiResponse<AnswerResponse>.SuccessResponse(data:result,statusCode:200));
        }

        [HttpPost("terminate")]
        [Authorize]
        public async Task<ActionResult<string>> Terminate(CancellationToken cancellationToken)
        {
            var result = await _ragService.TerminateAsync(cancellationToken);
            return Ok(ApiResponse<string>.SuccessResponse(message: "Terminated successfully", statusCode: 200));
        }
    }
}