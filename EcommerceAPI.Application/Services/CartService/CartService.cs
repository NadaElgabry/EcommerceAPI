using EcommerceAPI.Application.DTOs.Cart;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Services.CartService
{
    public class CartService : ICartService
    {
        private readonly ICurrentUserService _currentUserService ;
        private readonly IRepository<Product> _productRepository ;
        private readonly IRepository<User> _userRepository ;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserActivityService _userActivityService;

        public CartService(ICurrentUserService currentUserService,IRepository<Product> productRepository,
            IRepository<User> userSerivce, IUnitOfWork unitOfWork, IRepository<Cart> cartRepository,
            IUserActivityService userActivityService)
        {
            _currentUserService = currentUserService;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _userRepository = userSerivce;
            _userActivityService= userActivityService;
        }

        public async Task<int> AddToCart(AddToCartRequest request, CancellationToken cancellationToken)
        {
            bool newCart = false;
            var user = await _userRepository.GetByAsync(u => u.Guid == _currentUserService.UserGuid, cancellationToken)
                ?? throw new NotFoundException("User not Found");
            var product = await _productRepository.GetByAsync(p => p.Slug == request.ProductSlug, cancellationToken)
                ?? throw new NotFoundException("Product not Found"); ;
            var cart = await _cartRepository.GetByAsync(predicate: c => c.UserId == user.Id, cancellationToken: cancellationToken, include: query => query.Include(c => c.Items));
            var existingItem = cart?.Items.FirstOrDefault(i => i.ProductId == product.Id);
            var requestedTotal = request.Quantity + (existingItem?.Quantity ?? 0);

            if (requestedTotal > product.StockQuantity)
                throw new InsufficientStockException("There is no enough quantity in stock");

            if (cart is null)
            {
                cart = new Cart { UserId = user.Id };
                newCart = true;
            }

            if (existingItem is not null)
                existingItem.Quantity += request.Quantity;
            else
                cart.Items.Add(new CartItem { ProductId = product.Id, Quantity = request.Quantity, UnitPrice = product.Price });
            cart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (newCart)
                {
                    await _cartRepository.AddAsync(cart, cancellationToken);
                }
                else
                {
                    _cartRepository.Update(cart);
                }
                await _userActivityService.LogActivityAsync(user.Id, product.Id, UserActionType.AddToCart, cancellationToken);
            }
            ,cancellationToken);

            return cart.Items.Sum(i => i.Quantity);
        }
    }
}
