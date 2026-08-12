using EcommerceAPI.Application.DTOs.User;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Iservices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Application.Mappers.Mappings;
using EcommerceAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IUserMapper _userMapper;
        private readonly ICurrentUserService _currentUserService;
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
    }
}
