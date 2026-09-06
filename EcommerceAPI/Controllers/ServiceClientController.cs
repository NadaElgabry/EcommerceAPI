using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.ServiceAuth;
using EcommerceAPI.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/service-clients")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ServiceClientController : ControllerBase
    {
        private readonly IServiceClientService _serviceClientService;

        public ServiceClientController(IServiceClientService serviceClientService)
        {
            _serviceClientService = serviceClientService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServiceClientRequest request, CancellationToken cancellationToken)
        {
            var result = await _serviceClientService.CreateAsync(request, cancellationToken);
            return Ok(ApiResponse<CreateServiceClientResponse>.SuccessResponse(
                message: "Service client created — store this secret now, it will not be shown again.",
                statusCode: 201,
                data: result));
        }

        [HttpDelete("{clientId}")]
        public async Task<IActionResult> Revoke([FromRoute] string clientId, CancellationToken cancellationToken)
        {
            await _serviceClientService.RevokeAsync(clientId, cancellationToken);
            return NoContent();
        }
    }
}