using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.User;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
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
        public async Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
        {
            if (_currentUserService.Role != "Admin")
            {
                if (_currentUserService.UserGuid != userId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to access this resource.");
                }
            }
            var user = await _userRepository.GetByAsync(
                    u => u.Guid == userId,
                    cancellationToken)
                    ?? throw new NotFoundException("User not found.");

                user = _userMapper.UpdateUserFromRequest(user, request);
                await _unitOfWork.ExecuteInTransactionAsync(
                    async () =>
                    {
                        _userRepository.Update(user);

                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }, cancellationToken);
            return _userMapper.ToUserResponse(user);           
        }

        public async Task<OffsetPagedResult<UserResponse>> GetAllUsersAsync(
    OffsetPageRequest request,
    CancellationToken cancellationToken = default)
        {
            int page = Math.Max(request.PageNumber, 1);
            int pageSize = Math.Clamp(request.PageSize, 1, 100);

            var users = await _userRepository.GetPageOffSetAsync(
                orderBy: u => u.Id,
                skip: (page - 1) * pageSize,
                take: pageSize,
                cancellationToken: cancellationToken);

            var totalCount = await _userRepository.GetCountAsync(cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new OffsetPagedResult<UserResponse>
            {
                Data = users.Select(u => _userMapper.ToUserResponse(u)).ToList(),
                Pagination = new PageInfo
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasNext = page < totalPages,
                    HasPrevious = page > 1
                }
            };
        }
    }
}
