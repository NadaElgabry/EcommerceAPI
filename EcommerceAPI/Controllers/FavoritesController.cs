using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Favorites;
using EcommerceAPI.Application.DTOs.Product;
using EcommerceAPI.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/favorites")]
    [ApiController]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoritesService _favoritesService;

        public FavoritesController(IFavoritesService favoritesService)
        {
            _favoritesService = favoritesService;
        }

        [HttpGet("products")]
        [ProducesResponseType(typeof(ApiResponse<List<FavoriteProductResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFavoriteProducts(
            [FromQuery] string? cursor, [FromQuery] int pageSize, CancellationToken cancellationToken)
        {
            var result = await _favoritesService.GetFavoriteProductsAsync(cursor, pageSize, cancellationToken);
            return Ok(ApiResponse<CursorPagedResult<FavoriteProductResponse>>.SuccessResponse(
                message: "Favorite products retrieved successfully.", statusCode: 200, data: result));
        }

        [HttpPost("products/{slug}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddFavoriteProduct([FromRoute] string slug, CancellationToken cancellationToken)
        {
            await _favoritesService.AddFavoriteProductAsync(slug, cancellationToken);
            return Ok(ApiResponse<string>.SuccessResponse(
                message: "Product added to favorites successfully.",
                statusCode: 200));
        }

        [HttpDelete("products/{slug}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveFavoriteProduct([FromRoute] string slug, CancellationToken cancellationToken)
        {
            await _favoritesService.RemoveFavoriteProductAsync(slug, cancellationToken);
            return Ok(ApiResponse<string>.SuccessResponse(
                message: "Product removed from favorites successfully.",
                statusCode: 200));
        }

        [HttpGet("categories")]
        [ProducesResponseType(typeof(ApiResponse<List<FavoriteCategoryResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFavoriteCategories(CancellationToken cancellationToken)
        {
            var result = await _favoritesService.GetFavoriteCategoriesAsync(cancellationToken);
            return Ok(ApiResponse<List<FavoriteCategoryResponse>>.SuccessResponse(
                message: "Favorite categories retrieved successfully.", statusCode: 200, data: result));
        }

        [HttpPost("categories/{slug}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddFavoriteCategory([FromRoute] string slug, CancellationToken cancellationToken)
        {
            await _favoritesService.AddFavoriteCategoryAsync(slug, cancellationToken);
            return Ok(ApiResponse<string>.SuccessResponse(
                message: "Category added to favorites successfully.",
                statusCode: 200));
        }

        [HttpDelete("categories/{slug}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveFavoriteCategory([FromRoute] string slug, CancellationToken cancellationToken)
        {
            await _favoritesService.RemoveFavoriteCategoryAsync(slug, cancellationToken);
            return Ok(ApiResponse<string>.SuccessResponse(
                message: "Category removed from favorites successfully.",
                statusCode: 200));
        }
    }
}