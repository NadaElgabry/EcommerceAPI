using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Category;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Product;
using EcommerceAPI.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(
            ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateCategory(
            [FromForm] CreateCategoryRequest request,
            CancellationToken cancellationToken)
        {
            var categoryResponse =
                await _categoryService.CreateCategoryAsync(
                    request,
                    cancellationToken);

            return Created(
                "api/categories",
                ApiResponse<CategoryResponse>.SuccessResponse(
                    statusCode: 201,
                    message: "Category created successfully.",
                    data: categoryResponse));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<CategoryResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories(
            [FromQuery] GetCategoriesRequest request,
            CancellationToken cancellationToken)
        {
            var categories =
                await _categoryService.GetCategoriesAsync(
                    request,
                    cancellationToken);

            return Ok(
                ApiResponse<CursorPagedResult<CategoryResponse>>
                    .SuccessResponse(
                        statusCode: 200,
                        message: "Categories retrieved successfully.",
                        data: categories));
        }

        [HttpGet("{slug}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategoryDetails(
            string slug,
            CancellationToken cancellationToken)
        {
            var category =
                await _categoryService.GetCategoryDetailsAsync(
                    slug,
                    cancellationToken);

            return Ok(
                ApiResponse<CategoryResponse>.SuccessResponse(
                    statusCode: 200,
                    message: "Category retrieved successfully.",
                    data: category));
        }

        [HttpGet("{slug}/products")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<List<ProductResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategoryProducts(
            string slug,
            [FromQuery] GetCategoriesRequest request,
            CancellationToken cancellationToken)
        {
            var products =
                await _categoryService.GetCategoryProductsAsync(
                    slug,
                    request,
                    cancellationToken);

            return Ok(
                ApiResponse<CursorPagedResult<ProductSummaryResponse>>
                    .SuccessResponse(
                        statusCode: 200,
                        message: "Category products retrieved successfully.",
                        data: products));
        }

        [HttpPut("{slug}")]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateCategory(
            string slug,
            [FromForm] UpdateCategoryRequest request,
            CancellationToken cancellationToken)
        {
            var categoryResponse =
                await _categoryService.UpdateCategoryAsync(
                    slug,
                    request,
                    cancellationToken);

            return Ok(
                ApiResponse<CategoryResponse>.SuccessResponse(
                    statusCode: 200,
                    message: "Category updated.",
                    data: categoryResponse));
        }

        [HttpDelete("{slug}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(
            string slug,
            CancellationToken cancellationToken)
        {
            await _categoryService.DeleteCategoryAsync(
                slug,
                cancellationToken);

            return NoContent();
        }
    }
}