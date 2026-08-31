using EcommerceAPI.Application.DTOs.Cart;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Mappers.Interfaces;
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

        private readonly ICartMapper _cartMapper ;

        public CartService(ICurrentUserService currentUserService,IRepository<Product> productRepository,
            IRepository<User> userSerivce, IUnitOfWork unitOfWork, IRepository<Cart> cartRepository,
            IUserActivityService userActivityService, ICartMapper cartMapper)
        {
            _currentUserService = currentUserService;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _userRepository = userSerivce;
            _userActivityService= userActivityService;
            _cartMapper = cartMapper;
        }

        public async Task<int> AddToCart(AddToCartRequest request, CancellationToken cancellationToken)
        {
            bool newCart = false;
            var user = await GetActiveUserAsync(cancellationToken);
            var product = await GetProductBySlugAsync(request.ProductSlug, cancellationToken);
            var cart = await GetCartWithItemsAsync(user.Id, cancellationToken);
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
            {
                existingItem.Quantity += request.Quantity;
                existingItem.RefreshPrice(product.Price);
            }
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

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            ,cancellationToken);

            return cart.Items.Sum(i => i.Quantity);
        }

        public async Task<CartResponse> GetCart(CancellationToken cancellationToken)
        {
            var user = await GetActiveUserAsync(cancellationToken);
            var cart = await GetCartWithItemsAsync(user.Id, cancellationToken);

            if (cart is null)
                return new CartResponse { Items = new List<CartItemResponse>() };
            var changedItemIds = new List<int>();

            foreach (var item in cart.Items)
            {
                if (item.RefreshPrice(item.Product.Price))
                    changedItemIds.Add(item.Id);
            }

            if (changedItemIds.Any())
            {
                cart.UpdatedAt = DateTime.UtcNow;
                _cartRepository.Update(cart);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return _cartMapper.ToCartResponse(cart, changedItemIds);
        }
        public async Task<CartItemResponse?> UpdateCart(UpdateCartRequest request, CancellationToken cancellationToken)
        {
            var user = await GetActiveUserAsync(cancellationToken);
            var product = await GetProductBySlugAsync(request.ProductSlug, cancellationToken);
            var cart = await GetCartWithItemsAsync(user.Id, cancellationToken)
                ?? throw new NotFoundException("Cart not Found");

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == product.Id)
                ?? throw new NotFoundException("Item not Found in cart");

            bool isRemoval = request.Quantity == 0;

            if (isRemoval)
            {
                cart.Items.Remove(existingItem);
                existingItem = null;
            }
            else
            {
                EnsureSufficientStock(request.Quantity, product.StockQuantity);
                existingItem.Quantity = request.Quantity;
                existingItem.RefreshPrice(product.Price);
            }

            cart.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                _cartRepository.Update(cart);
                if(isRemoval)
                    await _userActivityService.LogActivityAsync(
                                                user.Id,
                                                product.Id,
                                                UserActionType.RemoveFromCart,
                                                cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);

            return existingItem is null ? null : _cartMapper.ToCartItemResponse(existingItem);
        }
        private async Task<User> GetActiveUserAsync(CancellationToken cancellationToken)
        {
            return await _userRepository.GetByAsync(
                u => u.Guid == _currentUserService.UserGuid && u.IsActive,
                cancellationToken)
                ?? throw new NotFoundException("User not Found");
        }

        private async Task<Product> GetProductBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            return await _productRepository.GetByAsync(p => p.Slug == slug, cancellationToken)
                ?? throw new NotFoundException("Product not Found");
        }

        private async Task<Cart> GetCartWithItemsAsync(int userId, CancellationToken cancellationToken)
        {
            return await _cartRepository.GetByAsync(
                predicate: c => c.UserId == userId,
                cancellationToken: cancellationToken,
                include: query => query.Include(c => c.Items));
        }

        private static void EnsureSufficientStock(int requestedQuantity, int availableStock)
        {
            if (requestedQuantity > availableStock)
                throw new InsufficientStockException("There is no enough quantity in stock");
        }
    }
}
