using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Order;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface IOrderService
    {
        public Task<OrderResponse> PlaceOrderAsync(PlaceOrderRequest request, string idempotencyKey, CancellationToken cancellationToken);
        public Task<CursorPagedResult<OrderSummary>> GetOrdersAsync(Guid userGuid, GetOrdersRequest request, CancellationToken cancellationToken);
        public Task<OrderResponse> GetOrderByGuidAsync(Guid orderGuid, CancellationToken cancellationToken);
        public Task<OrderResponse> UpdateOrderStatusAsync(Guid orderGuid, UpdateOrderStatusRequest request, CancellationToken cancellationToken);
    }
}