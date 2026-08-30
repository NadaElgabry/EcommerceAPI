using EcommerceAPI.Application.DTOs.Order;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface IOrderService
    {
        public Task<OrderResponse> PlaceOrderAsync(PlaceOrderRequest request, string idempotencyKey, CancellationToken cancellationToken);
        public Task<OrderResponse> GetOrderByGuidAsync(Guid orderGuid, CancellationToken cancellationToken);
    }
}
