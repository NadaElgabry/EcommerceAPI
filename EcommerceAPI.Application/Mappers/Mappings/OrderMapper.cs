using EcommerceAPI.Application.DTOs.Order;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Domain.Enums;

namespace EcommerceAPI.Application.Mappers
{
    public class OrderMapper : IOrderMapper
    {
        public Order ToEntity(PlaceOrderRequest request, Cart cart, int userId, string idempotencyKey)
        {
            return new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                Address = request.Address,
                Status = OrderStatus.Pending,
                IdempotencyKey = idempotencyKey,
                TotalAmount = cart.Items.Sum(i => i.Quantity * i.UnitPrice),
                Items = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    product = i.Product,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
        }

        public OrderResponse ToOrderResponse(Order order)
        {
            return new OrderResponse
            {
                Guid = order.Guid,
                OrderNumber = order.OrderNumber,
                Address = order.Address,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                Items = order.Items.Select(i => new OrderItemResponse
                {
                    ProductName = i.product.Name,
                    ProductSlug = i.product.Slug,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
        }
        private static string GenerateOrderNumber()
        {
            var randomDigits = Random.Shared.Next(100000, 999999);
            return randomDigits.ToString();
        }
    }
}