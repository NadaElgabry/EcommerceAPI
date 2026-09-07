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
                    Product = i.Product,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    ProductName = i.Product.Name,
                    ProductSlug = i.Product.Slug,
                    ProductDescription = i.Product.Description,
                    ProductImageUrl = i.Product.ProductImage,
                    ProductAltText = i.Product.AltText
                }).ToList()
            };
        }
        public OrderItemResponse ToOrderItemResponse(OrderItem item)
        {
            return new OrderItemResponse
            {
                Slug = item.ProductSlug,
                Name = item.ProductName,
                Quantity = item.Quantity,
                ProductImage = item.ProductImageUrl,
                AltText = item.ProductAltText,
                UnitPrice = item.UnitPrice,
                Description = item.ProductDescription,
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
                CreationDate = order.CreationDate,
                TotalAmount = order.TotalAmount,
                Items = order.Items.Select(oi => ToOrderItemResponse(oi)).ToList()
            };
        }
        public OrderSummary ToOrderSummary(Order order)
        {
            return new OrderSummary
            {
                Guid = order.Guid,
                OrderNumber= order.OrderNumber,
                Total = order.TotalAmount,
                Address = order.Address,
                CreationDate = order.CreationDate,
                DeliveryTime = order.DeliveryTime,
                Status = order.Status.ToString(),
                TotalItems = order.Items.Count()
            };
        }

        private static string GenerateOrderNumber()
        {
            var randomDigits = Random.Shared.Next(100000, 999999);
            return randomDigits.ToString();
        }
    }
}
