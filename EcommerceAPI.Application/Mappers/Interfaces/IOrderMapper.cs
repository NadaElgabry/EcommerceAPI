using EcommerceAPI.Application.DTOs.Order;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Mappers.Interfaces
{
    public interface IOrderMapper
    {
        Order ToEntity(PlaceOrderRequest request, Cart cart, int userId, string idempotencyKey);
        OrderResponse ToOrderResponse(Order order);
        public OrderSummary ToOrderSummary(Order order);
        public OrderItemResponse ToOrderItemResponse(OrderItem item);
    }
}