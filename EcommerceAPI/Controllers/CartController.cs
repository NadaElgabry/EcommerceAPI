using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Cart;
using EcommerceAPI.Application.Interfaces.IServices;
using IdempotentAPI.Filters;
using Microsoft.AspNetCore.Authorization;
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
        [Idempotent(ExpiresInMilliseconds =1000*20)]
        [ProducesResponseType(typeof(ApiResponse<CartResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddtoCart([FromBody] AddToCartRequest request,CancellationToken cancellationToken)
        {
            var itemCount = await _cartService.AddToCart(request, cancellationToken);
            return Ok(ApiResponse<int>.SuccessResponse(data: itemCount, message: "Added to cart.", statusCode: 200));
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CartResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCart(CancellationToken cancellationToken) 
        {
            var result = await _cartService.GetCart(cancellationToken);
            return Ok(ApiResponse<CartResponse>.SuccessResponse(data:result, message:"Cart retrived successfuly",statusCode: 200));
        }

        [HttpPut]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CartResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCart([FromBody] UpdateCartRequest request,CancellationToken cancellationToken)
        {
            var result = await _cartService.UpdateCart(request, cancellationToken);
            var message = result is null ? "Product removed from cart." : "Product updated successfully.";
            return Ok(ApiResponse<CartItemResponse?>.SuccessResponse(
                data: result, message: message,statusCode:200));
        }
    }
}
