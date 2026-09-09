using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Order;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Interfaces.Search;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace EcommerceAPI.Application.Services.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IUserActivityService _userActivityService;
        private readonly IOrderMapper _orderMapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductIndexingService _productIndexingService;
        private readonly  ILogger<OrderService> _logger;

        public OrderService(
            ICurrentUserService currentUserService,
            IRepository<User> userRepository,
            IRepository<Cart> cartRepository,
            IRepository<Order> orderRepository,
            IRepository<Product> productRepository,
            IUserActivityService userActivityService,
            IOrderMapper orderMapper,
            IUnitOfWork unitOfWork,
            IProductIndexingService productIndexingService,
            ILogger<OrderService> logger)
        {
            _currentUserService = currentUserService;
            _userRepository = userRepository;
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userActivityService = userActivityService;
            _orderMapper = orderMapper;
            _unitOfWork = unitOfWork;
            _productIndexingService = productIndexingService;
            _logger = logger;
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

                    await _userActivityService.LogActivityAsync(
                        userId: user.Id,
                        productId: item.Product.Id,
                        actionType: UserActionType.PlaceOrder,
                        cancellationToken: cancellationToken);
                }

                await _orderRepository.AddAsync(order, cancellationToken);
                _cartRepository.Delete(cart);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
            foreach (var item in cart.Items)
            {
                try
                {
                    await _productIndexingService.IndexProductAsync(item.Product, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to reindex product {ProductId} after order placement. Stock data is out of sync with search until next reindex.", item.Product.Id);
                }
            }

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
                include: query => query.Include(o=>o.Items).Include(o => o.User),
                orderBy: o => o.CreationDate, take: take + 1,
                cancellationToken: cancellationToken
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
            var isAdmin = _currentUserService.Role == "Admin";

            Expression<Func<Order, bool>> predicate = isAdmin
                ? o => o.Guid == orderGuid
                : o => o.Guid == orderGuid && o.User.Guid == _currentUserService.UserGuid;

            var order = await _orderRepository.GetByAsync(
                predicate: predicate,
                include: query => query.Include(o => o.Items),
                cancellationToken: cancellationToken)
                ?? throw new NotFoundException("Order not found");

            return _orderMapper.ToOrderResponse(order);
        }
        public async Task<CursorPagedResult<OrderSummary>> GetAllOrdersAsync(GetAllOrdersRequest request, CancellationToken cancellationToken)
        {
            OrderStatus? statusFilter = null;
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (!Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var parsed))
                    throw new BadRequestException("Invalid order status.");
                statusFilter = parsed;
            }

            var lastId = string.IsNullOrEmpty(request.Cursor) ? 0 : CursorHelper.Decode<int>(request.Cursor);
            var take = Math.Clamp(request.Limit, 1, 50);

            var orders = await _orderRepository.GetPagedAsync(
                predicate: o => o.Id > lastId && (statusFilter == null || o.Status == statusFilter),
                orderBy: o => o.Id,
                take: take + 1,
                include: query => query.Include(o => o.Items).Include(o=>o.User),
                cancellationToken: cancellationToken);

            var hasNext = orders.Count > take;
            if (hasNext) orders.RemoveAt(orders.Count - 1);

            var summaries = orders.Select(o => _orderMapper.ToOrderSummary(o)).ToList();

            return new CursorPagedResult<OrderSummary>
            {
                Data = summaries,
                Pagination = new CursorPageInfo
                {
                    NextCursor = hasNext && summaries.Count > 0 ? CursorHelper.Encode(orders[^1].Id) : null,
                    HasNext = hasNext,
                    PageSize = summaries.Count
                }
            };
        }
        public async Task<OrderResponse> UpdateOrderStatusAsync(
            Guid orderGuid,
            UpdateOrderStatusRequest request,
            CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByAsync(
                predicate: o => o.Guid == orderGuid,
                include: query => query.Include(o => o.Items).ThenInclude(i => i.Product),
                cancellationToken: cancellationToken)
                ?? throw new NotFoundException("Order not found");

            if (!Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var newStatus))
            {
                throw new BadRequestException("Invalid order status.");
            }

            if (!IsValidStatusTransition(order.Status, newStatus))
            {
                throw new BadRequestException(
                    $"Order status cannot be changed from {order.Status} to {newStatus}.");
            }

            order.Status = newStatus;

            if (newStatus == OrderStatus.Delivered)
            {
                order.DeliveryTime = DateTime.UtcNow;
            }

            var restockedProducts = new List<Product>();

            if (newStatus == OrderStatus.Cancelled)
            {
                foreach (var item in order.Items)
                {
                    if (item.Product == null) continue;

                    item.Product.StockQuantity += item.Quantity;
                    _productRepository.Update(item.Product);
                    restockedProducts.Add(item.Product);
                }
            }

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                _orderRepository.Update(order);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);

            foreach (var product in restockedProducts)
            {
                try
                {
                    await _productIndexingService.IndexProductAsync(product, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to reindex product {ProductId} after order cancellation. Stock data is out of sync with search until next reindex.", product.Id);
                }
            }

            return _orderMapper.ToOrderResponse(order);
        }

        private static bool IsValidStatusTransition(
            OrderStatus currentStatus,
            OrderStatus newStatus)
        {
            return currentStatus switch
            {
                OrderStatus.Pending =>
                    newStatus == OrderStatus.Placed,

                OrderStatus.Placed =>
                    newStatus == OrderStatus.Shipped ||
                    newStatus == OrderStatus.Cancelled,

                OrderStatus.Shipped =>
                    newStatus == OrderStatus.Delivered,

                OrderStatus.Delivered => false,

                OrderStatus.Cancelled => false,

                _ => false
            };
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