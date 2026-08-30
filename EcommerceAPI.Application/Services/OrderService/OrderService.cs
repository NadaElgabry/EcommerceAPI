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
        private readonly IOrderMapper _orderMapper;
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(
            ICurrentUserService currentUserService,
            IRepository<User> userRepository,
            IRepository<Cart> cartRepository,
            IRepository<Order> orderRepository,
            IOrderMapper orderMapper,
            IUnitOfWork unitOfWork)
        {
            _currentUserService = currentUserService;
            _userRepository = userRepository;
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _orderMapper = orderMapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<OrderResponse> PlaceOrderAsync(PlaceOrderRequest request, string idempotencyKey, CancellationToken cancellationToken)
        {
            var user = await GetActiveUserAsync(cancellationToken);
            var cart = await GetCartWithItemsAsync(user.Id, cancellationToken)
                ?? throw new NotFoundException("Cart not Found");

            if (!cart.Items.Any())
                throw new InvalidOperationException("Cannot place an order with an empty cart.");

            var order = _orderMapper.ToEntity(request, cart, user.Id, idempotencyKey);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _orderRepository.AddAsync(order, cancellationToken);

                _cartRepository.Delete(cart);
            }, cancellationToken);

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