using EcommerceAPI.Application.DTOs.Cart;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface ICartService
    {
        public Task<int> AddToCart(AddToCartRequest request,CancellationToken cancellationToken);
        public Task<CartResponse> GetCart(CancellationToken cancellationToken);
    }
}
