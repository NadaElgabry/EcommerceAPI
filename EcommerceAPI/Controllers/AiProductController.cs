using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Product;
using EcommerceAPI.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/ai/products")]
    [ApiController]
    [Authorize(Policy = "ProductsRead")]
    public class AiProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public AiProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [ProducesResponseType(
            typeof(ApiResponse<List<AiProductResponse>>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetProducts(
            CancellationToken cancellationToken)
        {
            var products =
                await _productService.GetProductsForAiAsync(
                    cancellationToken);

            return Ok(
                ApiResponse<List<AiProductResponse>>
                    .SuccessResponse(
                        statusCode: 200,
                        message: "Products retrieved successfully.",
                        data: products));
        }
    }
}