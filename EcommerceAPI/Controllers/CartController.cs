using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Cart;
using EcommerceAPI.Application.Interfaces.IServices;
using IdempotentAPI.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpPost]
        [Authorize]
        [Idempotent(ExpiresInMilliseconds =1000*30)]
        public async Task<IActionResult> AddtoCart([FromBody] AddToCartRequest request,CancellationToken cancellationToken)
        {
            var itemCount = await _cartService.AddToCart(request, cancellationToken);
            return Ok(ApiResponse<int>.SuccessResponse(data: itemCount, message: "Added to cart.", statusCode: 200));
        }
    }
}
