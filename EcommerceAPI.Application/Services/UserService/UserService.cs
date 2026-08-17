using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.User;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Iservices;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IUserMapper _userMapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        private record UserCursor(DateTime CreatedAt, int Id);

        public UserService(
            IRepository<User> userRepository,
            ITokenService tokenService,
            IUserMapper userMapper,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService
            )
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _userMapper = userMapper;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        /// <inheritdoc />
        public async Task<UserResponse> GetUserProfileAsync(Guid userGuid, CancellationToken cancellationToken = default)
        {
            if (_currentUserService.Role != "Admin")
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

        /// <inheritdoc />
        public async Task UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
        {
            if (_currentUserService.Role != "Admin")
            {
                if (_currentUserService.UserGuid != userId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to access this resource.");
                }

                var user = await _userRepository.GetByAsync(
                    u => u.Guid == userId,
                    cancellationToken)
                    ?? throw new NotFoundException("User not found.");

                _userMapper.UpdateUserFromRequest(user, request);

                _userRepository.Update(user);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
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
