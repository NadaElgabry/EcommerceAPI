using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.User;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Iservices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Mappers.Interfaces;

using EcommerceAPI.Application.Common;

namespace EcommerceAPI.Application.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IUserMapper _userMapper;
        private readonly ICurrentUserService _currentUserService;

        private record UserCursor(DateTime CreatedAt, int Id);

        public UserService(IRepository<User> userRepository, IUserMapper userMapper, ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _userMapper = userMapper;
            _currentUserService = currentUserService;
        }

        /// <inheritdoc />
        public async Task<UserResponse> GetUserProfileAsync(Guid userGuid, CancellationToken cancellationToken = default)
        {
            if(_currentUserService.Role != "Admin")
            {
                if (_currentUserService.UserGuid != userGuid)
                {
                    throw new UnauthorizedAccessException("You are not authorized to access this resource.");
                }
            }

            var user = await _userRepository.GetByAsync(
                predicate: u => u.Guid == userGuid,
                cancellationToken: cancellationToken)
                ?? throw new NotFoundException("User not found.");

            return _userMapper.ToUserResponse(user);
        }
        public Task<PagedResult<UserResponse>> GetAllUsersAsync(
    GetUsersRequest request,
    CancellationToken cancellationToken = default)
        {
            return CursorPaginator.PaginateAsync(
                repository: _userRepository,

                after: request.After,
                before: request.Before,

                pageSize: request.PageSize,

                defaultCursor: new UserCursor(
                    DateTime.MinValue,
                    int.MinValue),

                forwardPredicate: c =>
                    u =>
                        u.CreatedAt > c.CreatedAt ||
                        (u.CreatedAt == c.CreatedAt &&
                         u.Id > c.Id),

                backwardPredicate: c =>
                    u =>
                        u.CreatedAt < c.CreatedAt ||
                        (u.CreatedAt == c.CreatedAt &&
                         u.Id < c.Id),

                orderBy: u => u.CreatedAt,
                thenBy: u => u.Id,

                selectCursor: u =>
                    new UserCursor(
                        u.CreatedAt,
                        u.Id),

                map: _userMapper.ToUserResponse,

                cancellationToken: cancellationToken);
        }
    }
}
