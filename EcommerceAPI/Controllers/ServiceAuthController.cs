using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.ServiceAuth;
using EcommerceAPI.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/auth/service")]
    [ApiController]
    public class ServiceAuthController : ControllerBase
    {
        private readonly IServiceClientService _serviceClientService;

        public ServiceAuthController(IServiceClientService serviceClientService)
        {
            _serviceClientService = serviceClientService;
        }

        [HttpPost("token")]
        public async Task<IActionResult> GetToken([FromBody] ServiceTokenRequest request, CancellationToken cancellationToken)
        {
            var result = await _serviceClientService.IssueTokenAsync(request, cancellationToken);
            return Ok(ApiResponse<ServiceTokenResponse>.SuccessResponse(
                message: "Service token issued",
                statusCode: 200,
                data: result));
        }
    }
}