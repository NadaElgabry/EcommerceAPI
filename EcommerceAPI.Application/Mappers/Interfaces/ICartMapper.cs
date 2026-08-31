using EcommerceAPI.Application.DTOs.Cart;
using EcommerceAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Mappers.Interfaces
{
    public interface ICartMapper
    {
        CartItemResponse ToCartItemResponse(CartItem cartItem, bool priceChanged = false);
        CartResponse ToCartResponse(Cart cart, IEnumerable<int> changedItemIds = null);

    }
}
