using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Product;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Search;
using EcommerceAPI.Application.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ProductController> _logger;
        public ProductController(
            IProductService productService,
            IServiceScopeFactory scopeFactory,
            ILogger<ProductController> logger)
        {
            _productService = productService;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductRequest request, CancellationToken cancellationToken)
        {
            var productResponse = await _productService.CreateProductAsync(request, cancellationToken);
            return Created(
                "api/products",
                ApiResponse<ProductResponse>.SuccessResponse(
                    statusCode: 201,
                    message: "Product created successfully.",
                    data: productResponse));
        }

        [HttpGet("{slug}")]
        [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductDetails([FromRoute] string slug, CancellationToken cancellationToken)
        {

            var productResponse = await _productService.GetProductDetailsAsync(slug, cancellationToken);

            return Ok(
                ApiResponse<ProductResponse>.SuccessResponse(
                    statusCode: 200,
                    message: "Product retrieved successfully.",
                    data: productResponse));
        }

        [HttpPut("{slug}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateProduct(
            [FromRoute] string slug, [FromForm] UpdateProductRequest request, CancellationToken cancellationToken)
        {
            var result = await _productService.UpdateProductAsync(
                slug,
                request,
                cancellationToken);

            return Ok(
                ApiResponse<ProductResponse>.SuccessResponse(
                    message: "Product updated successfully",
                    statusCode: 200,
                    data: result));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<CursorPagedResult<ProductSummaryResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchProducts([FromQuery] ProductQueryParamsRequest request, CancellationToken cancellationToken)
        {
            var result = await _productService.SearchProductsAsync(request, cancellationToken);
            return Ok(ApiResponse<CursorPagedResult<ProductSummaryResponse>>.SuccessResponse(
                message: "Products retrieved successfully",
                statusCode: 200,
                data: result));
        }

        [HttpPost("visual-search")]
        [ProducesResponseType(typeof(ApiResponse<List<ProductSummaryResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> VisualSearch([FromForm] IFormFile file, [FromQuery] int? top_k, CancellationToken ct)
        {
            var results = await _productService.VisualSearchAsync(file, top_k, ct);
            return Ok(ApiResponse<List<ProductSummaryResponse>>.SuccessResponse(200, "Visual search results.", results));
        }

        [HttpDelete("{slug}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteProduct(
            [FromRoute] string slug, CancellationToken cancellationToken)
        {
            await _productService.DeleteProductAsync(slug, cancellationToken);
            return StatusCode(
                204,
                ApiResponse<string>.SuccessResponse(message: "Product deleted successfully",
                statusCode: 204));
        }
        [HttpPost("reindex")]
        [Authorize(Roles = "Admin")]
        public IActionResult ReindexAll()
        {
            _ = RunReindexInBackgroundAsync();

            return Accepted(
                ApiResponse<string>.SuccessResponse(
                    statusCode: 202,
                    message: "Product reindex started."));
        }
        private async Task RunReindexInBackgroundAsync()
        {
            // New DI scope because this outlives the HTTP request that kicked it off.
            using var scope = _scopeFactory.CreateScope();
            var indexingService = scope.ServiceProvider.GetRequiredService<IProductIndexingService>();

            try
            {
                await indexingService.ReindexAllProductsAsync();
                _logger.LogInformation("Product reindex completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Product reindex failed.");
            }
        }
    }
}
