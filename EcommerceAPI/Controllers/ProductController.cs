using EcommerceAPI.Application.Common;
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
    }
}
