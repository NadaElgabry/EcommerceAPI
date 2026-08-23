using EcommerceAPI.Application.Common;
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

        /*        
        [HttpGet("products")]
        public async Task<ActionResult<List<ProductResponse>>> GetFavoriteProducts(CancellationToken cancellationToken)
        {
        }
        */

        [HttpPost("products/{slug}")]
        public async Task<IActionResult> AddFavoriteProduct([FromRoute] string slug, CancellationToken cancellationToken)
        {
            await _favoritesService.AddFavoriteProductAsync(slug, cancellationToken);
            return Ok(ApiResponse<string>.SuccessResponse(
                message: "Product added to favorites successfully.",
                statusCode: 200));
        }

        [HttpDelete("products/{slug}")]
        public async Task<IActionResult> RemoveFavoriteProduct([FromRoute] string slug, CancellationToken cancellationToken)
        {
            await _favoritesService.RemoveFavoriteProductAsync(slug, cancellationToken);
            return Ok(ApiResponse<string>.SuccessResponse(
                message: "Product removed from favorites successfully.",
                statusCode: 200));
        }

        /*        
        [HttpGet("categories")]
        public async Task<ActionResult<List<ProductResponse>>> GetFavoriteCategories(CancellationToken cancellationToken)
        {
        }
        */

        [HttpPost("categories/{slug}")]
        public async Task<IActionResult> AddFavoriteCategory([FromRoute] string slug, CancellationToken cancellationToken)
        {
            await _favoritesService.AddFavoriteCategoryAsync(slug, cancellationToken);
            return Ok(ApiResponse<string>.SuccessResponse(
                message: "Category added to favorites successfully.",
                statusCode: 200));
        }

        [HttpDelete("categories/{slug}")]
        public async Task<IActionResult> RemoveFavoriteCategory([FromRoute] string slug, CancellationToken cancellationToken)
        {
            await _favoritesService.RemoveFavoriteCategoryAsync(slug, cancellationToken);
            return Ok(ApiResponse<string>.SuccessResponse(
                message: "Category removed from favorites successfully.",
                statusCode: 200));
        }
    }
}