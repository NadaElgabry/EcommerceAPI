using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Email;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Runtime;

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
        private readonly IVerificationEmailTemplateProvider _templateProvider;
        public AuthService(
            IRepository<User> userRepository,
            IRepository<VerificationToken> verificationTokenRepository,
            IRepository<RefreshToken> refreshTokenRepository,
            IAuthMapper authMapper,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IUnitOfWork unitOfWork,
            ILogger<AuthService> logger,
            IEmailService emailService,
            IVerificationEmailTemplateProvider templateProvider)
        {
            _userRepository = userRepository;
            _verificationTokenRepository = verificationTokenRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _authMapper = authMapper;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _emailService = emailService;
            _templateProvider = templateProvider;
        }

        /// <inheritdoc />
        public async Task CreateUserAsync(
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

            var token = _tokenService.GenerateVerificationToken(user, VerificationPurpose.EmailVerification);


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
            await SendVerificationEmailAsync(user, token.RawToken,
                VerificationPurpose.EmailVerification, cancellationToken);
        }

        /// <inheritdoc />
        public async Task ResendEmailAsync(ResendEmailRequest request,
            CancellationToken cancellationToken = default)
        {
            string normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var user = await _userRepository.GetByAsync(
                predicate: u => u.Email == normalizedEmail,
                include: query => query.Include(u => u.VerificationTokens),
                cancellationToken: cancellationToken)
                ?? throw new NotFoundException("User not found.");

            var activeToken = user.VerificationTokens
                .Where(vt => vt.Purpose == request.Purpose
                          && !vt.ConsumedAt.HasValue
                          && vt.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefault()
                ?? throw new NotFoundException("No active verification token found.");

            var newToken = _tokenService.GenerateVerificationToken(user, request.Purpose);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                _verificationTokenRepository.Delete(activeToken);
                await _verificationTokenRepository.AddAsync(newToken.Entity, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);

            await SendVerificationEmailAsync(user, newToken.RawToken, request.Purpose, cancellationToken);
        }

        /// <inheritdoc />
        private async Task SendVerificationEmailAsync(User user, string rawToken,
            VerificationPurpose purpose, CancellationToken cancellationToken)
        {
            var (subject, body) = _templateProvider.GetTemplate(purpose, rawToken);
            await _emailService.SendEmailAsync(user.Email, subject, body, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> ActivateEmailAsync(
            ActivateEmailRequest request, CancellationToken cancellationToken = default)
        {
            var token = await _verificationTokenRepository.GetByAsync(
                predicate: vt => vt.TokenHash == _tokenService.Hash(request.Token) 
                && !vt.ConsumedAt.HasValue && vt.ExpiresAt > DateTime.UtcNow 
                && vt.Purpose == VerificationPurpose.EmailVerification,
                include: query => query.Include(vt => vt.User),
                cancellationToken: cancellationToken)
                ?? throw new NotFoundException("Invalid activation token.");


            token.User.isActive = true;
            token.ConsumedAt = DateTime.UtcNow;

            var accesstoken = _tokenService.GenerateAccessToken(token.User);

            var (rawToken, newRefreshToken) = _tokenService.GenerateRefreshToken(token.User);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);

            return true;

        }

        /// <inheritdoc />
        public async Task<bool> IsEmailAvailable(EmailRequest request, CancellationToken cancellationToken = default)
        {
            var isValid = await _userRepository.ExistByAsync(u => u.Email == request.Email.ToLower(), cancellationToken);
            return !(isValid);
        }

        /// <inheritdoc />
        public async Task<AuthResponse> Login
            (LoginRequest request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByAsync(u => u.Email == request.Email.Trim().ToLower(), cancellationToken)
                ?? throw new UnauthorizedException("Invalid credentials");

            if(!user.isActive)
                throw new UnauthorizedException("User is not Activated");

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
                Role = user.Role,
                UserId = user.Guid
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
                RefreshTokenExpiresAtUtc = newRefreshToken.ExpiresAt,
                Role = newRefreshToken.User.Role,
                UserId = newRefreshToken.User.Guid
            };
        }

        /// <inheritdoc/>
        public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var user = await _userRepository.GetByAsync(u => u.Email == normalizedEmail, cancellationToken);

            // Not throwing error if the user doesnt exist to avoid giving away information about registered users
            if (user == null)
            {
                _logger.LogInformation("Password reset requested for a non-existent user with email: {Email}", normalizedEmail);
                return;
            }

            var (rawCode, TokenEntity) = _tokenService.GenerateVerificationToken(user, VerificationPurpose.PasswordReset);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _verificationTokenRepository.AddAsync(TokenEntity, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);

            await SendVerificationEmailAsync(user, rawCode,
                VerificationPurpose.PasswordReset, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<VerifyResetCodeResponse> VerifyResetCodeAsync(VerifyResetCodeRequest request, CancellationToken cancellationToken = default)
        {
            var hashedCode = _tokenService.Hash(request.Code);

            var storedToken = await _verificationTokenRepository.GetByAsync(
                predicate: vt => vt.TokenHash == hashedCode && vt.Purpose == VerificationPurpose.PasswordReset,
                include: query => query.Include(vt => vt.User),
                cancellationToken: cancellationToken);

            if (storedToken == null || !storedToken.IsActive)
            {
                throw new BadRequestException("Invalid or expired reset code.");
            }

            var newResetToken = _tokenService.GenerateHighEntropyToken();

            storedToken.TokenHash = _tokenService.Hash(newResetToken);
            storedToken.ExpiresAt = DateTime.UtcNow.AddMinutes(10);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new VerifyResetCodeResponse
            {
                ResetToken = newResetToken,
            };

        }

        /// <inheritdoc />
        public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
        {
            var hashedToken = _tokenService.Hash(request.ResetToken);

            var storedToken = await _verificationTokenRepository.GetByAsync(predicate: vt => vt.TokenHash == hashedToken && vt.Purpose == Domain.Enums.VerificationPurpose.PasswordReset,
                include: query => query.Include(vt => vt.User),
                cancellationToken: cancellationToken);

            if(storedToken == null || !storedToken.IsActive)
            {
                throw new BadRequestException("Invalid or expired reset token. Please verify your email again.");
            }

            var user = storedToken.User;
            user.HashedPassword = _passwordHasher.Hash(request.NewPassword);
            storedToken.ConsumedAt = DateTime.UtcNow;

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _refreshTokenRepository.DeleteAllByAsync(
                    rt => rt.UserId == user.Id,
                    cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

            }, cancellationToken);
        }
    }
}