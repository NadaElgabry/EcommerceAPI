/*// AuthServiceTests.cs
//
// Full unit test coverage for EcommerceAPI.Application.Services.Auth.AuthService.
//
// Stack: xUnit + Moq
// NuGet packages needed in the test project:
//   - xunit
//   - xunit.runner.visualstudio
//   - Moq
//   - Microsoft.EntityFrameworkCore (for IIncludableQueryable used by IRepository<T>)
//
// NOTE ON DOMAIN ENTITIES:
// User, RefreshToken, and VerificationToken all have plain public setters, EXCEPT
// IsActive on RefreshToken/VerificationToken, which is a computed property:
//   RefreshToken.IsActive       => DateTime.UtcNow < ExpiresAt
//   VerificationToken.IsActive  => ConsumedAt == null && DateTime.UtcNow < ExpiresAt
// The factory helpers below set ExpiresAt/ConsumedAt to drive IsActive rather than
// assigning it directly (it has no setter).

using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Email;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Application.Services.Auth;
using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EcommerceAPI.Application.Tests.Services.Auth
{
    public class AuthServiceTests
    {
        private readonly Mock<IRepository<User>> _userRepository = new();
        private readonly Mock<IRepository<VerificationToken>> _verificationTokenRepository = new();
        private readonly Mock<IRepository<RefreshToken>> _refreshTokenRepository = new();
        private readonly Mock<IAuthMapper> _authMapper = new();
        private readonly Mock<IPasswordHasher> _passwordHasher = new();
        private readonly Mock<ITokenService> _tokenService = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<ILogger<AuthService>> _logger = new();
        private readonly Mock<IEmailService> _emailService = new();

        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            // Make ExecuteInTransactionAsync actually invoke the delegate passed to it,
            // so the code under test runs exactly as it would in production.
            _unitOfWork
                .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
                .Returns<Func<Task>, CancellationToken>((operation, _) => operation());

            _sut = new AuthService(
                _userRepository.Object,
                _verificationTokenRepository.Object,
                _refreshTokenRepository.Object,
                _authMapper.Object,
                _passwordHasher.Object,
                _tokenService.Object,
                _unitOfWork.Object,
                _logger.Object,
                _emailService.Object);
        }

        // ---------------------------------------------------------------
        // Test helpers / factories
        //
        // RefreshToken.IsActive and VerificationToken.IsActive are computed
        // (based on ExpiresAt / ConsumedAt), not stored — so to control them
        // in a test we set ExpiresAt (and ConsumedAt) into the past or future
        // rather than assigning IsActive itself.
        // ---------------------------------------------------------------

        private static User CreateUser(string email = "user@example.com", string hashedPassword = "hashed", bool IsActive = false) => new()
        {
            Email = email,
            HashedPassword = hashedPassword,
            IsActive = IsActive
        };

        private static RefreshToken CreateRefreshToken(User user, bool IsActive = true, string tokenHash = "hashed-refresh") => new()
        {
            User = user,
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = IsActive ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddDays(-1)
        };

        private static VerificationToken CreateVerificationToken(User user, bool IsActive = true, string tokenHash = "hashed-activation") => new()
        {
            User = user,
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = IsActive ? DateTime.UtcNow.AddDays(1) : DateTime.UtcNow.AddDays(-1),
            ConsumedAt = null
        };

        // =================================================================
        // CreateUserAsync
        // =================================================================

        [Fact]
        public async Task CreateUserAsync_WhenEmailIsNew_CreatesUserAndReturnsRawActivationToken()
        {
            // Arrange
            var request = new RegisterRequest
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "  Jane.Doe@Example.com ",
                PhoneNumber = "  555-1234  ",
                Password = "Sup3rSecret!"
            };

            var mappedUser = CreateUser(email: "jane.doe@example.com");

            _userRepository
                .Setup(r => r.ExistByAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _authMapper
                .Setup(m => m.ToUser(It.IsAny<RegisterRequest>()))
                .Returns(mappedUser);

            _passwordHasher
                .Setup(p => p.Hash("Sup3rSecret!"))
                .Returns("hashed-password");

            var verificationToken = CreateVerificationToken(mappedUser);
            _tokenService
                .Setup(t => t.GenerateActivationToken(mappedUser))
                .Returns(("raw-activation-token", verificationToken));

            // Act
            var result = await _sut.CreateUserAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal("raw-activation-token", result);
            Assert.Equal("jane.doe@example.com", request.Email); // normalized in place
            Assert.Equal("555-1234", request.PhoneNumber);       // trimmed in place

            _userRepository.Verify(r => r.AddAsync(mappedUser, It.IsAny<CancellationToken>()), Times.Once);
            _verificationTokenRepository.Verify(r => r.AddAsync(verificationToken, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWork.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_WhenEmailAlreadyExists_ThrowsConflictException()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "existing@example.com",
                PhoneNumber = "5551234",
                Password = "password"
            };

            _userRepository
                .Setup(r => r.ExistByAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => _sut.CreateUserAsync(request, CancellationToken.None));

            _authMapper.Verify(m => m.ToUser(It.IsAny<RegisterRequest>()), Times.Never);
            _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWork.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // =================================================================
        // ActivateEmailAsync
        // =================================================================

        [Fact]
        public async Task ActivateEmailAsync_WhenTokenIsValid_ActivatesUserAndReturnsAuthResponse()
        {
            // Arrange
            var request = new ActivateEmailRequest { Token = "raw-token" };
            var user = CreateUser(IsActive: false);
            var verificationToken = CreateVerificationToken(user, IsActive: true, tokenHash: "hashed-token");

            _tokenService.Setup(t => t.Hash("raw-token")).Returns("hashed-token");

            _verificationTokenRepository
                .Setup(r => r.GetByAsync(
                    It.IsAny<Expression<Func<VerificationToken, bool>>>(),
                    It.IsAny<Func<IQueryable<VerificationToken>, IIncludableQueryable<VerificationToken, object>>?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(verificationToken);

            var accessTokenResult = new AccessTokenResult("access-token", DateTime.UtcNow.AddMinutes(15));
            _tokenService.Setup(t => t.GenerateAccessToken(user)).Returns(accessTokenResult);

            var refreshToken = CreateRefreshToken(user);
            _tokenService.Setup(t => t.GenerateRefreshToken(user)).Returns(("raw-refresh-token", refreshToken));

            // Act
            var response = await _sut.ActivateEmailAsync(request, CancellationToken.None);

            // Assert
            Assert.True(user.IsActive);
            Assert.NotNull(verificationToken.ConsumedAt);
            Assert.Equal("access-token", response.AccessToken);
            Assert.Equal("raw-refresh-token", response.RefreshToken);
            Assert.Equal(accessTokenResult.ExpiresAtUtc, response.AccessTokenExpiresAtUtc);
            Assert.Equal(refreshToken.ExpiresAt, response.RefreshTokenExpiresAtUtc);

            _refreshTokenRepository.Verify(r => r.AddAsync(refreshToken, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ActivateEmailAsync_WhenTokenNotFound_ThrowsNotFoundException()
        {
            var request = new ActivateEmailRequest { Token = "bad-token" };

            _tokenService.Setup(t => t.Hash("bad-token")).Returns("hashed-bad-token");

            _verificationTokenRepository
                .Setup(r => r.GetByAsync(
                    It.IsAny<Expression<Func<VerificationToken, bool>>>(),
                    It.IsAny<Func<IQueryable<VerificationToken>, IIncludableQueryable<VerificationToken, object>>?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((VerificationToken?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.ActivateEmailAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task ActivateEmailAsync_WhenTokenIsNotActive_ThrowsNotFoundException()
        {
            var request = new ActivateEmailRequest { Token = "expired-token" };
            var user = CreateUser();
            var verificationToken = CreateVerificationToken(user, IsActive: false, tokenHash: "hashed-expired-token");

            _tokenService.Setup(t => t.Hash("expired-token")).Returns("hashed-expired-token");

            _verificationTokenRepository
                .Setup(r => r.GetByAsync(
                    It.IsAny<Expression<Func<VerificationToken, bool>>>(),
                    It.IsAny<Func<IQueryable<VerificationToken>, IIncludableQueryable<VerificationToken, object>>?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(verificationToken);

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.ActivateEmailAsync(request, CancellationToken.None));
        }

        // =================================================================
        // IsEmailAvailable
        // =================================================================

        [Fact]
        public async Task IsEmailAvailable_WhenNoUserWithEmailExists_ReturnsTrue()
        {
            _userRepository
                .Setup(r => r.ExistByAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _sut.IsEmailAvailable(new EmailRequest { Email = "free@example.com" }, CancellationToken.None);

            Assert.True(result);
        }

        [Fact]
        public async Task IsEmailAvailable_WhenUserWithEmailExists_ReturnsFalse()
        {
            _userRepository
                .Setup(r => r.ExistByAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _sut.IsEmailAvailable(new EmailRequest { Email = "taken@example.com" }, CancellationToken.None);

            Assert.False(result);
        }

        // =================================================================
        // Login
        // =================================================================

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsAuthResponse()
        {
            var request = new LoginRequest { Email = "user@example.com", Password = "correct-password" };
            var user = CreateUser(email: "user@example.com", hashedPassword: "hashed-password", IsActive: true);

            _userRepository
                .Setup(r => r.GetByAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _passwordHasher
                .Setup(p => p.Verify("correct-password", "hashed-password"))
                .Returns(true);

            var accessTokenResult = new AccessTokenResult("access-token", DateTime.UtcNow.AddMinutes(15));
            _tokenService.Setup(t => t.GenerateAccessToken(user)).Returns(accessTokenResult);

            var refreshToken = CreateRefreshToken(user);
            _tokenService.Setup(t => t.GenerateRefreshToken(user)).Returns(("raw-refresh-token", refreshToken));

            var response = await _sut.Login(request, CancellationToken.None);

            Assert.Equal("access-token", response.AccessToken);
            Assert.Equal("raw-refresh-token", response.RefreshToken);
            _refreshTokenRepository.Verify(r => r.AddAsync(refreshToken, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Login_WhenUserDoesNotExist_ThrowsUnauthorizedException()
        {
            var request = new LoginRequest { Email = "missing@example.com", Password = "whatever" };

            _userRepository
                .Setup(r => r.GetByAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.Login(request, CancellationToken.None));

            _passwordHasher.Verify(p => p.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Login_WhenPasswordIsInvalid_ThrowsUnauthorizedException()
        {
            var request = new LoginRequest { Email = "user@example.com", Password = "wrong-password" };
            var user = CreateUser(email: "user@example.com", hashedPassword: "hashed-password");

            _userRepository
                .Setup(r => r.GetByAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _passwordHasher
                .Setup(p => p.Verify("wrong-password", "hashed-password"))
                .Returns(false);

            await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.Login(request, CancellationToken.None));

            _tokenService.Verify(t => t.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        }

        // =================================================================
        // Logout
        // =================================================================

        [Fact]
        public async Task Logout_WhenTokenExists_DeletesTokenAndSaves()
        {
            var request = new LogoutRequest { RefreshToken = "raw-refresh-token" };
            var user = CreateUser();
            var storedToken = CreateRefreshToken(user, tokenHash: "hashed-refresh-token");

            _tokenService.Setup(t => t.Hash("raw-refresh-token")).Returns("hashed-refresh-token");

            _refreshTokenRepository
                .Setup(r => r.GetByAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(storedToken);

            await _sut.Logout(request, CancellationToken.None);

            _refreshTokenRepository.Verify(r => r.Delete(storedToken), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Logout_WhenTokenDoesNotExist_IsIdempotentAndDoesNotThrow()
        {
            var request = new LogoutRequest { RefreshToken = "already-gone" };

            _tokenService.Setup(t => t.Hash("already-gone")).Returns("hashed-already-gone");

            _refreshTokenRepository
                .Setup(r => r.GetByAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((RefreshToken?)null);

            // Should complete without throwing.
            await _sut.Logout(request, CancellationToken.None);

            _refreshTokenRepository.Verify(r => r.Delete(It.IsAny<RefreshToken>()), Times.Never);
            _unitOfWork.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // =================================================================
        // Refresh
        // =================================================================

        [Fact]
        public async Task Refresh_WithValidActiveToken_RotatesTokenAndReturnsAuthResponse()
        {
            var request = new RefreshTokenRequest { RefreshToken = "old-raw-token" };
            var user = CreateUser();
            var storedToken = CreateRefreshToken(user, IsActive: true, tokenHash: "hashed-old-token");

            _tokenService.Setup(t => t.Hash("old-raw-token")).Returns("hashed-old-token");

            _refreshTokenRepository
                .Setup(r => r.GetByAsync(
                    It.IsAny<Expression<Func<RefreshToken, bool>>>(),
                    It.IsAny<Func<IQueryable<RefreshToken>, IIncludableQueryable<RefreshToken, object>>?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(storedToken);

            var accessTokenResult = new AccessTokenResult("new-access-token", DateTime.UtcNow.AddMinutes(15));
            _tokenService.Setup(t => t.GenerateAccessToken(user)).Returns(accessTokenResult);

            var newRefreshToken = CreateRefreshToken(user, tokenHash: "hashed-new-token");
            _tokenService.Setup(t => t.GenerateRefreshToken(user)).Returns(("new-raw-token", newRefreshToken));

            var response = await _sut.Refresh(request, CancellationToken.None);

            Assert.Equal("new-access-token", response.AccessToken);
            Assert.Equal("new-raw-token", response.RefreshToken);

            _refreshTokenRepository.Verify(r => r.Delete(storedToken), Times.Once);
            _refreshTokenRepository.Verify(r => r.AddAsync(newRefreshToken, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Refresh_WhenTokenNotFound_ThrowsUnauthorizedException()
        {
            var request = new RefreshTokenRequest { RefreshToken = "unknown-token" };

            _tokenService.Setup(t => t.Hash("unknown-token")).Returns("hashed-unknown-token");

            _refreshTokenRepository
                .Setup(r => r.GetByAsync(
                    It.IsAny<Expression<Func<RefreshToken, bool>>>(),
                    It.IsAny<Func<IQueryable<RefreshToken>, IIncludableQueryable<RefreshToken, object>>?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((RefreshToken?)null);

            await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.Refresh(request, CancellationToken.None));
        }

        [Fact]
        public async Task Refresh_WhenTokenIsInactive_ThrowsUnauthorizedException()
        {
            var request = new RefreshTokenRequest { RefreshToken = "inactive-token" };
            var user = CreateUser();
            var storedToken = CreateRefreshToken(user, IsActive: false, tokenHash: "hashed-inactive-token");

            _tokenService.Setup(t => t.Hash("inactive-token")).Returns("hashed-inactive-token");

            _refreshTokenRepository
                .Setup(r => r.GetByAsync(
                    It.IsAny<Expression<Func<RefreshToken, bool>>>(),
                    It.IsAny<Func<IQueryable<RefreshToken>, IIncludableQueryable<RefreshToken, object>>?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(storedToken);

            await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.Refresh(request, CancellationToken.None));

            _refreshTokenRepository.Verify(r => r.Delete(It.IsAny<RefreshToken>()), Times.Never);
        }
    }
}*/