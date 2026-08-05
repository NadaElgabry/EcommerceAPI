using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<RefreshToken> _refreshTokenRepository;
        private readonly IAuthMapper _authMapper;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IRepository<User> userRepository,
            IRepository<RefreshToken> refreshTokenRepository,
            IAuthMapper authMapper,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IUnitOfWork unitOfWork,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _refreshTokenRepository =
                refreshTokenRepository;
            _authMapper = authMapper;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<AuthResponse> CreateUserAsync(
            RegisterRequest request,
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

            user.HashedPassword = _passwordHasher.Hash(request.Password);

            var refreshTokenResult = _tokenService.GenerateRefreshToken(user);

            refreshTokenResult.Entity.User = user;

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
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
            }, cancellationToken);

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
                    refreshTokenResult.Entity.ExpiresAt,
            };
        }

        /// <inheritdoc />
        public async Task<AuthResponse> Login
            (LoginRequest request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByAsync(u => u.Email == request.Email.ToLower(), cancellationToken)
                ?? throw new UnauthorizedException("Invalid credentials");
            if (!_passwordHasher.Verify(request.Password, user.HashedPassword))
                throw new UnauthorizedException("Invalid credentials");

            var accesstoken = _tokenService.GenerateAccessToken(user);
            var storedToken = await _refreshTokenRepository
                .GetByAsync(rt => rt.UserId == user.Id , cancellationToken);
            
            var (rawToken, newRefreshToken) = _tokenService.GenerateRefreshToken(user);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (null != storedToken)
                {
                    _refreshTokenRepository.Delete(storedToken);
                }
                await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
            
            

            

            return new AuthResponse
            {
                AccessToken = accesstoken.Token,
                AccessTokenExpiresAtUtc = accesstoken.ExpiresAtUtc,
                RefreshToken = rawToken,
                RefreshTokenExpiresAtUtc = newRefreshToken.ExpiresAt,
            };
        }

        /// <inheritdoc />
        public async Task Logout(LogoutRequest request, CancellationToken cancellationToken = default)
        {
            var hashedToken = _tokenService.HashRefreshToken(request.RefreshToken);

            var storedToken = await _refreshTokenRepository.GetByAsync(
                rt => rt.TokenHash == hashedToken,
                cancellationToken);

            if (storedToken == null)
            {

                // Logout is idempotent by design
                _logger.LogInformation(
                "Logout requested for a refresh token that no longer exists (already logged out, expired, or invalid). TokenHashPrefix: {TokenHashPrefix}",
                hashedToken[..Math.Min(8, hashedToken.Length)]);
                return;
            }

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                _refreshTokenRepository.Delete(storedToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);

        }

        /// <inheritdoc />
        public async Task<AuthResponse> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            var hashedToken = _tokenService.HashRefreshToken(request.RefreshToken);

            var storedToken = await _refreshTokenRepository.GetByAsync(
                predicate: rt => rt.TokenHash == hashedToken,
                include: query => query.Include(rt => rt.User));

            if (storedToken == null || !storedToken.IsActive)
            {
                throw new UnauthorizedException("Invalid or expired refresh token.");
            }

            var accessTokenResult = _tokenService.GenerateAccessToken(storedToken.User);

            var (rawToken, newRefreshToken) = _tokenService.GenerateRefreshToken(storedToken.User);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                _refreshTokenRepository.Delete(storedToken);
                await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);


            return new AuthResponse
            {
                AccessToken = accessTokenResult.Token,
                AccessTokenExpiresAtUtc = accessTokenResult.ExpiresAtUtc,
                RefreshToken = rawToken,
                RefreshTokenExpiresAtUtc = newRefreshToken.ExpiresAt
            };
        }
    }
}