using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Order;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Application.Services.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IOrderMapper _orderMapper;
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(
            ICurrentUserService currentUserService,
            IRepository<User> userRepository,
            IRepository<Cart> cartRepository,
            IRepository<Order> orderRepository,
            IRepository<Product> productRepository,
            IOrderMapper orderMapper,
            IUnitOfWork unitOfWork)
        {
            _currentUserService = currentUserService;
            _userRepository = userRepository;
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _orderMapper = orderMapper;
            _unitOfWork = unitOfWork;
        }


        public async Task<OrderResponse> PlaceOrderAsync(PlaceOrderRequest request, string idempotencyKey, CancellationToken cancellationToken)
        {
            var user = await GetActiveUserAsync(cancellationToken);
            var cart = await GetCartWithItemsAsync(user.Id, cancellationToken)
                ?? throw new NotFoundException("Cart not Found");

            if (await _orderRepository.ExistByAsync(predicate: o => o.IdempotencyKey == idempotencyKey, cancellationToken: cancellationToken))
            {
                throw new ConflictException("An order with the same idempotency key already exists.");
            }

            if (!cart.Items.Any())
                throw new BadRequestException("Cannot place an order with an empty cart.");

            var priceChangedItems = cart.Items
                .Where(i => i.UnitPrice != i.Product.Price)
                .Select(i => i.Product.Name)
                .ToList();

            if (priceChangedItems.Any())
            {
                throw new ConflictException(
                    $"The price has changed for the following items: {string.Join(", ", priceChangedItems)}. Please review your cart before checking out.");
            }

            var insufficientItems = cart.Items
                .Where(i => i.Quantity > i.Product.StockQuantity)
                .Select(i => i.Product.Name)
                .ToList();

            if (insufficientItems.Any())
            {
                throw new InsufficientStockException(
                    $"Insufficient stock for: {string.Join(", ", insufficientItems)}");
            }

            var order = _orderMapper.ToEntity(request, cart, user.Id, idempotencyKey);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {

                foreach (var item in cart.Items)
                {
                    item.Product.StockQuantity -= item.Quantity;
                    _productRepository.Update(item.Product);
                }

                await _orderRepository.AddAsync(order, cancellationToken);
                _cartRepository.Delete(cart);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);

            return _orderMapper.ToOrderResponse(order);
        }
        public async Task<CursorPagedResult<OrderSummary>> GetOrdersAsync(Guid userGuid, GetOrdersRequest request, CancellationToken cancellationToken)
        {
            if (_currentUserService.Role != "Admin")
            {
                if (_currentUserService.UserGuid != userGuid)
                {
                    throw new UnauthorizedAccessException("You are not authorized to access this resource.");
                }
            }
            var user = await _userRepository.GetByAsync(predicate: u => u.Guid == userGuid
            , cancellationToken) ?? throw new NotFoundException("User not found");

            var lastOrderId = string.IsNullOrEmpty(request.Cursor) ? 0 : CursorHelper.Decode<int>(request.Cursor);
            var take = Math.Clamp(request.Limit, 1, 50);
            var orders = await _orderRepository.GetPagedAsync(predicate: o => o.UserId == user.Id && o.Id > lastOrderId,
                orderBy: o => o.CreationDate, take: take + 1, include: query => query.Include(c => c.Items)
            );

            var hasNext = orders.Count > request.Limit;

            if (hasNext)
            {
                orders.RemoveAt(orders.Count - 1);
            }
            string? nextCursor = null;

            if (hasNext && orders.Count > 0)
            {
                nextCursor = CursorHelper.Encode(
                    orders[^1].CreationDate);
            }
            var ordersummaries = orders.Select(o => _orderMapper.ToOrderSummary(o)).ToList();

            return new CursorPagedResult<OrderSummary>
            {
                Data = ordersummaries,

                Pagination = new CursorPageInfo
                {
                    NextCursor = nextCursor,
                    HasNext = hasNext,
                    PageSize = ordersummaries.Count
                }
            };
        }

        public async Task<OrderResponse> GetOrderByGuidAsync(Guid orderGuid, CancellationToken cancellationToken)
        {
            var user = await GetActiveUserAsync(cancellationToken);

            var order = await _orderRepository.GetByAsync(
                predicate: o => o.Guid == orderGuid && o.UserId == user.Id,
                include: query => query.Include(o => o.Items).ThenInclude(i => i.Product),
                cancellationToken: cancellationToken)
                ?? throw new NotFoundException("Order not found");

            return _orderMapper.ToOrderResponse(order);
        }

        private async Task<User> GetActiveUserAsync(CancellationToken cancellationToken)
        {
            return await _userRepository.GetByAsync(
                u => u.Guid == _currentUserService.UserGuid && u.IsActive,
                cancellationToken)
                ?? throw new NotFoundException("User not Found");
        }

        private async Task<Cart> GetCartWithItemsAsync(int userId, CancellationToken cancellationToken)
        {
            return await _cartRepository.GetByAsync(
                predicate: c => c.UserId == userId,
                cancellationToken: cancellationToken,
                include: query => query.Include(c => c.Items).ThenInclude(i => i.Product));
        }
    }
}