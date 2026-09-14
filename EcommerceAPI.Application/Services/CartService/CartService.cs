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
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EcommerceAPI.Application.Services.CartService
{
    public class CartService : ICartService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserActivityService _userActivityService;
        private readonly ICartMapper _cartMapper;
        private readonly ILogger<CartService> _logger;

        public CartService(ICurrentUserService currentUserService, IRepository<Product> productRepository,
            IRepository<User> userSerivce, IUnitOfWork unitOfWork, IRepository<Cart> cartRepository,
            IUserActivityService userActivityService, ICartMapper cartMapper, ILogger<CartService> logger)
        {
            _currentUserService = currentUserService;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _userRepository = userSerivce;
            _userActivityService = userActivityService;
            _cartMapper = cartMapper;
            _logger = logger;
        }

        public async Task<int> AddToCart(AddToCartRequest request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            long last = 0;
            bool newCart = false;

            var user = await GetActiveUserAsync(cancellationToken);
            _logger.LogInformation("[AddToCart] Fetched user - took {StepMs}ms (total {TotalMs}ms)",
                stopwatch.ElapsedMilliseconds - last, stopwatch.ElapsedMilliseconds);
            last = stopwatch.ElapsedMilliseconds;

            var product = await GetProductBySlugAsync(request.ProductSlug, cancellationToken);
            _logger.LogInformation("[AddToCart] Fetched product - took {StepMs}ms (total {TotalMs}ms)",
                stopwatch.ElapsedMilliseconds - last, stopwatch.ElapsedMilliseconds);
            last = stopwatch.ElapsedMilliseconds;

            var cart = await GetCartWithItemsAsync(user.Id, cancellationToken);
            _logger.LogInformation("[AddToCart] Fetched cart - took {StepMs}ms (total {TotalMs}ms)",
                stopwatch.ElapsedMilliseconds - last, stopwatch.ElapsedMilliseconds);
            last = stopwatch.ElapsedMilliseconds;

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
                existingItem.Quantity += (int) request.Quantity;
                existingItem.RefreshPrice(product.Price);
            }
            else
                cart.Items.Add(new CartItem { ProductId = product.Id, Quantity = (int) request.Quantity, UnitPrice = product.Price });
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
            }, cancellationToken);
            _logger.LogInformation("[AddToCart] Transaction (save + activity log) - took {StepMs}ms (total {TotalMs}ms)",
                stopwatch.ElapsedMilliseconds - last, stopwatch.ElapsedMilliseconds);

            stopwatch.Stop();
            _logger.LogInformation("[AddToCart] Completed - total {TotalMs}ms", stopwatch.ElapsedMilliseconds);

            return cart.Items.Sum(i => i.Quantity);
        }

        public async Task<CartResponse> GetCart(CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            long last = 0;

            var user = await GetActiveUserAsync(cancellationToken);
            _logger.LogInformation("[GetCart] Fetched user - took {StepMs}ms (total {TotalMs}ms)",
                stopwatch.ElapsedMilliseconds - last, stopwatch.ElapsedMilliseconds);
            last = stopwatch.ElapsedMilliseconds;

            var cart = await GetCartWithItemsAsync(user.Id, cancellationToken);
            _logger.LogInformation("[GetCart] Fetched cart - took {StepMs}ms (total {TotalMs}ms)",
                stopwatch.ElapsedMilliseconds - last, stopwatch.ElapsedMilliseconds);
            last = stopwatch.ElapsedMilliseconds;

            if (cart is null)
            {
                stopwatch.Stop();
                _logger.LogInformation("[GetCart] Completed (no cart) - total {TotalMs}ms", stopwatch.ElapsedMilliseconds);
                return new CartResponse { Items = new List<CartItemResponse>() };
            }

            var changedItemIds = new List<int>();

            foreach (var item in cart.Items)
            {
                if (item.RefreshPrice(item.Product.Price))
                    changedItemIds.Add(item.Id);
            }
            _logger.LogInformation("[GetCart] Price refresh check ({ItemCount} items) - took {StepMs}ms (total {TotalMs}ms)",
                cart.Items.Count, stopwatch.ElapsedMilliseconds - last, stopwatch.ElapsedMilliseconds);
            last = stopwatch.ElapsedMilliseconds;

            if (changedItemIds.Any())
            {
                cart.UpdatedAt = DateTime.UtcNow;
                _cartRepository.Update(cart);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("[GetCart] Saved price changes ({ChangedCount} items) - took {StepMs}ms (total {TotalMs}ms)",
                    changedItemIds.Count, stopwatch.ElapsedMilliseconds - last, stopwatch.ElapsedMilliseconds);
            }

            stopwatch.Stop();
            _logger.LogInformation("[GetCart] Completed - total {TotalMs}ms", stopwatch.ElapsedMilliseconds);

            return _cartMapper.ToCartResponse(cart, changedItemIds);
        }

        public async Task<CartItemResponse?> UpdateCart(UpdateCartRequest request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            long last = 0;

            var user = await GetActiveUserAsync(cancellationToken);
            _logger.LogInformation("[UpdateCart] Fetched user - took {StepMs}ms (total {TotalMs}ms)",
                stopwatch.ElapsedMilliseconds - last, stopwatch.ElapsedMilliseconds);
            last = stopwatch.ElapsedMilliseconds;

            var product = await GetProductBySlugAsync(request.ProductSlug, cancellationToken);
            _logger.LogInformation("[UpdateCart] Fetched product - took {StepMs}ms (total {TotalMs}ms)",
                stopwatch.ElapsedMilliseconds - last, stopwatch.ElapsedMilliseconds);
            last = stopwatch.ElapsedMilliseconds;

            var cart = await GetCartWithItemsAsync(user.Id, cancellationToken)
                ?? throw new NotFoundException("Cart not Found");
            _logger.LogInformation("[UpdateCart] Fetched cart - took {StepMs}ms (total {TotalMs}ms)",
                stopwatch.ElapsedMilliseconds - last, stopwatch.ElapsedMilliseconds);
            last = stopwatch.ElapsedMilliseconds;

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
                EnsureSufficientStock((int) request.Quantity, product.StockQuantity);
                existingItem.Quantity = (int) request.Quantity;
                existingItem.RefreshPrice(product.Price);
            }

            cart.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                _cartRepository.Update(cart);
                if (isRemoval)
                    await _userActivityService.LogActivityAsync(
                                                user.Id,
                                                product.Id,
                                                UserActionType.RemoveFromCart,
                                                cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
            _logger.LogInformation("[UpdateCart] Transaction (save{ActivityLog}) - took {StepMs}ms (total {TotalMs}ms)",
                isRemoval ? " + activity log" : "", stopwatch.ElapsedMilliseconds - last, stopwatch.ElapsedMilliseconds);

            stopwatch.Stop();
            _logger.LogInformation("[UpdateCart] Completed - total {TotalMs}ms", stopwatch.ElapsedMilliseconds);

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
                include: query => 
                query.Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Category)
                .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.ProductTags)
                .ThenInclude(pt => pt.Tag));
        }

        private static void EnsureSufficientStock(int requestedQuantity, int availableStock)
        {
            if (requestedQuantity > availableStock)
                throw new InsufficientStockException("There is no enough quantity in stock");
        }
    }
}