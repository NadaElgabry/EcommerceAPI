using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Product;
using EcommerceAPI.Application.Interfaces.IServices;
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

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> CreateCategory([FromForm] CreateProductRequest request, CancellationToken cancellationToken)
        {
            var productResponse = await _productService.CreateProductAsync(request, cancellationToken);
            return Created(
                "api/products",
                ApiResponse<ProductResponse>.SuccessResponse(
                    statusCode: 201,
                    message: "Product created successfully.",
                    data: productResponse));
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(
            [FromQuery] string? cursor,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _productService.GetProductsPagedAsync(cursor, pageSize, cancellationToken);

            return Ok(ApiResponse<CursorPagedResponse<ProductResponse>>.SuccessResponse(
                statusCode: 200,
                message: "Products fetched successfully.",
                data: result));
        }
    }
}
