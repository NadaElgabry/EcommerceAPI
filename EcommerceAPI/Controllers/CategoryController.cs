using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Category;
using EcommerceAPI.Application.DTOs.Common;
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
    }
}