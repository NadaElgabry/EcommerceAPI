using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Services.Auth;
using EcommerceAPI.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Application.Services.Users
{
    public class UsersService : IUsersService
    {
        private readonly IRepository<User> _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IUsersMapper _usersMapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UsersService(
            IRepository<User> userRepository,
            ITokenService tokenService,
            IUsersMapper usersMapper,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService
            )
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _usersMapper = usersMapper;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        /// <inheritdoc />
        public async Task UpdateProfileAsync(Guid? userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
        {
            if (userId.HasValue && _currentUserService.Role != "Admin")
            {
                throw new UnauthorizedException("Only administrators can specify a user ID.");                
            }
            if (!_currentUserService.IsAuthenticated)
            {
                throw new UnauthorizedException( "User is not authenticated.");
            }

            Guid targetUserId = userId ?? _currentUserService.UserGuid;

            var user = await _userRepository.GetByAsync(
                u => u.Guid == targetUserId,
                cancellationToken)
                ?? throw new NotFoundException("User not found.");

            _usersMapper.UpdateUserFromRequest(user, request);

            _userRepository.Update(user);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
