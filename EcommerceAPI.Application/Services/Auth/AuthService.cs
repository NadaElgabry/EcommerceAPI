using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Iservices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<RefreshToken> _refreshTokenRepository;
        private readonly IAuthMapper _authMapper;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(
            IRepository<User> userRepository,
            IRepository<Role> roleRepository,
            IRepository<RefreshToken> refreshTokenRepository,
            IAuthMapper authMapper,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _refreshTokenRepository =
                refreshTokenRepository;
            _authMapper = authMapper;
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


            var user = _authMapper.ToUser( request );

            user.CreatedAt = DateTime.UtcNow;
            user.HashedPassword = _passwordHasher.Hash(request.Password);
            user.Role = await _roleRepository.GetByAsync(predicate:r=>r.Id==1, cancellationToken);


            var refreshTokenResult = _tokenService.GenerateRefreshToken(user,ipAddress,deviceInfo);

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