using EcommerceAPI.Application.DTOs.Cart;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace EcommerceAPI.Application.Mappers.Mappings
{
    public class CartMapper : ICartMapper
    {
        public CartItemResponse ToCartItemResponse(CartItem cartItem, bool priceChanged = false)
        {
            return new CartItemResponse
            {
                Name = cartItem.Product.Name,
                Slug = cartItem.Product.Slug,
                AltText = cartItem.Product.AltText,
                ProductImageUrl = cartItem.Product.ProductImage,
                UnitPrice = cartItem.UnitPrice,
                Quantity = cartItem.Quantity,
                PriceChanged = priceChanged
            };
        }

        public CartResponse ToCartResponse(Cart cart, IEnumerable<int> changedItemIds = null)
        {
            var changedSet = changedItemIds?.ToHashSet() ?? new HashSet<int>();

            var items = cart.Items
                .Select(i => ToCartItemResponse(i, changedSet.Contains(i.Id)))
                .ToList();

            return new CartResponse
            {
                Items = items,
                Total = items.Sum(i => i.UnitPrice * i.Quantity)
            };
        }
    }
}