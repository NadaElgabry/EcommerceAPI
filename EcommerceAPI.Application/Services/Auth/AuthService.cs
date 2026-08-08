using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Email;
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
        private readonly IRepository<VerificationToken> _verificationTokenRepository;
        private readonly IRepository<RefreshToken> _refreshTokenRepository;
        private readonly IAuthMapper _authMapper;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailService _emailService;

        public AuthService(
            IRepository<User> userRepository,
            IRepository<VerificationToken> verificationTokenRepository,
            IRepository<RefreshToken> refreshTokenRepository,
            IAuthMapper authMapper,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IUnitOfWork unitOfWork,
            ILogger<AuthService> logger,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _refreshTokenRepository =
                refreshTokenRepository;
            _verificationTokenRepository = verificationTokenRepository;
            _authMapper = authMapper;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _emailService = emailService;
        }

        /// <inheritdoc />
        public async Task<string> CreateUserAsync(
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

            request.Email = normalizedEmail;
            request.PhoneNumber = normalizedPhoneNumber;

            var user = _authMapper.ToUser( request );

            user.HashedPassword = _passwordHasher.Hash(request.Password);

            var token = _tokenService.GenerateActivationToken(user);



            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _userRepository.AddAsync(
                user,
                cancellationToken
                );
                await _verificationTokenRepository.AddAsync(
                    token.Entity,
                    cancellationToken
                );
                await _unitOfWork.SaveChangesAsync(
                    cancellationToken
                );
                
            }, cancellationToken);
            /*await _emailService.SendEmailAsync(
                    user.Email,
                    "Activate your account",
                    $"Your activation code is: {token.RawToken}"
                );*/
            return token.RawToken;
        }

        public async Task<AuthResponse> ActivateEmailAsync(
            ActivateEmailRequest request, CancellationToken cancellationToken = default)
        {
            var token = await _verificationTokenRepository.GetByAsync(
                predicate: vt => vt.TokenHash == _tokenService.Hash(request.Token),
                include: query => query.Include(vt => vt.User),
                cancellationToken: cancellationToken);

            if(token == null || !token.IsActive ) {
                throw new NotFoundException("Invalid activation token.");
            }

            token.User.isActive = true;
            token.ConsumedAt = DateTime.UtcNow;

            var accesstoken = _tokenService.GenerateAccessToken(token.User);

            var (rawToken, newRefreshToken) = _tokenService.GenerateRefreshToken(token.User);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
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

        public async Task<bool> IsEmailAvailable(EmailRequest request, CancellationToken cancellationToken = default)
        {
            var isValid = await _userRepository.ExistByAsync(u => u.Email == request.Email.ToLower(), cancellationToken);
            return !(isValid);
        }

        /// <inheritdoc />
        public async Task<AuthResponse> Login
            (LoginRequest request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByAsync(u => u.Email == request.Email.Trim().ToLower() && u.isActive, cancellationToken)
                ?? throw new UnauthorizedException("Invalid credentials");
            if (!_passwordHasher.Verify(request.Password, user.HashedPassword))
                throw new UnauthorizedException("Invalid credentials");

            var accesstoken = _tokenService.GenerateAccessToken(user);
            
            var (rawToken, newRefreshToken) = _tokenService.GenerateRefreshToken(user);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
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
            var hashedToken = _tokenService.Hash(request.RefreshToken);

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
            var hashedToken = _tokenService.Hash(request.RefreshToken);

            var storedToken = await _refreshTokenRepository.GetByAsync(
                predicate: rt => rt.TokenHash == hashedToken,
                include: query => query.Include(rt => rt.User),
                cancellationToken: cancellationToken);

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