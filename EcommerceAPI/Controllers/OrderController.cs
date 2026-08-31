using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Order;
using EcommerceAPI.Application.Interfaces.IServices;
using IdempotentAPI.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        [Authorize]
        [Idempotent(ExpiresInMilliseconds = 1000 * 20)]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request,
            CancellationToken cancellationToken)
        {
            var idempotencyKey = Request.Headers["IdempotencyKey"].ToString();
            var orderResponse = await _orderService.PlaceOrderAsync(request,idempotencyKey, cancellationToken);
            return Ok(ApiResponse<OrderResponse>.SuccessResponse(data: orderResponse, message: $"Order #{orderResponse.OrderNumber} placed successfully!", statusCode: 200));
        }

        [HttpGet("{guid}")]
        [Authorize]
        public async Task<IActionResult> GetOrder([FromRoute] Guid guid, CancellationToken cancellationToken)
        {
            var response = await _orderService.GetOrderByGuidAsync(guid, cancellationToken);
            return Ok(ApiResponse<OrderResponse>.SuccessResponse(
                data: response,
                message: "Order retrieved successfully",
                statusCode: 200));
        }

        [HttpGet("user/{guid}")]
        [Authorize]
        public async Task<IActionResult> GetOrdersList([FromRoute] Guid guid,[FromQuery] GetOrdersRequest request,CancellationToken cancellationToken)
        {
            var response = await _orderService.GetOrdersAsync(guid,request,cancellationToken);
            return Ok(ApiResponse<CursorPagedResult<OrderSummary>>.SuccessResponse(
                data: response,
                message: "Orders retrieved successfully",
                statusCode: 200));
        }
    }
}
