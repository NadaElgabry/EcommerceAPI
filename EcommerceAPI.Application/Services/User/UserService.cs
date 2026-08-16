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
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IUserMapper _usersMapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UserService(
            IRepository<User> userRepository,
            ITokenService tokenService,
            IUserMapper usersMapper,
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

                _usersMapper.UpdateUserFromRequest(user, request);

                _userRepository.Update(user);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
