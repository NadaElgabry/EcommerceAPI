using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Iservices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Domain.Entities;
using UserEntity = EcommerceAPI.Domain.Entities.User;

namespace EcommerceAPI.Application.Services.User
{
    public class UserService : IUserService
    {
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<RefreshToken> _refreshTokenRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(
            IRepository<UserEntity> userRepository,
            IRepository<Role> roleRepository,
            IRepository<RefreshToken> refreshTokenRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _refreshTokenRepository =
                refreshTokenRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
        }

        public async Task<AuthResponse> CreateUserAsync(
            RegisterRequest request, string ipAddress, string deviceInfo,
            CancellationToken cancellationToken = default)
        {
            string normalizedEmail =
                request.Email.Trim().ToLowerInvariant();

            string normalizedPhoneNumber =
                request.PhoneNumber.Trim();

            bool emailExists =
                await _userRepository.ExistByAsync(
                    user => user.Email == normalizedEmail,
                    cancellationToken
                );

            if (emailExists)
            {
                throw new ConflictException(
                    "A user with this email already exists."
                );
            }

            bool phoneNumberExists =
                await _userRepository.ExistByAsync(
                    user =>
                        user.PhoneNumber ==
                        normalizedPhoneNumber,
                    cancellationToken
                );

            if (phoneNumberExists)
            {
                throw new ConflictException(
                    "A user with this phone number already exists."
                );
            }

            var customerRole =
                await _roleRepository.GetByAsync(
                    role => role.Name == "Customer",
                    cancellationToken
                );

            if (customerRole is null)
            {
                throw new InvalidOperationException(
                    "The Customer role does not exist in the database."
                );
            }

            var user = new UserEntity
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = normalizedEmail,
                PhoneNumber = normalizedPhoneNumber,
                HashedPassword =
                    _passwordHasher.Hash(request.Password),
                RoleId = customerRole.Id,
                Role = customerRole,
                CreatedAt = DateTime.UtcNow
            };

            var refreshTokenResult =
                _tokenService.GenerateRefreshToken(user,ipAddress,deviceInfo);

            refreshTokenResult.Entity.User = user;

            await _userRepository.AddAsync(
                user,
                cancellationToken
            );

            await _refreshTokenRepository.AddAsync(
                refreshTokenResult.Entity,
                cancellationToken
            );

            await _unitOfWork.SaveChangesAsync(
                cancellationToken
            );

            AccessTokenResult accessToken =
                _tokenService.GenerateAccessToken(user);

            return new AuthResponse
            {
                AccessToken = accessToken.Token,
                AccessTokenExpiresAtUtc =
                    accessToken.ExpiresAtUtc,
                RefreshToken =
                    refreshTokenResult.RawToken,
                RefreshTokenExpiresAtUtc =
                    refreshTokenResult.Entity.ExpiresAt
            };
        }
    }
}