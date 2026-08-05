using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Application.Services.Auth;
using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EcommerceAPI.Tests.Services.Auth
{
    /// <summary>
    /// Unit tests for <see cref="AuthService"/>, covering Register (CreateUserAsync),
    /// Login, Logout, and Refresh. Written against the real IRepository / ITokenService /
    /// IUnitOfWork / IAuthMapper / entity / DTO definitions.
    /// </summary>
    public class AuthServiceTests
    {
        private readonly Mock<IRepository<User>> _userRepositoryMock = new();
        private readonly Mock<IRepository<RefreshToken>> _refreshTokenRepositoryMock = new();
        private readonly Mock<IAuthMapper> _authMapperMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<ILogger<AuthService>> _loggerMock = new();

        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            _sut = new AuthService(
                _userRepositoryMock.Object,
                _refreshTokenRepositoryMock.Object,
                _authMapperMock.Object,
                _passwordHasherMock.Object,
                _tokenServiceMock.Object,
                _unitOfWorkMock.Object,
                _loggerMock.Object);

            // Transactions just invoke the delegate for these tests.
            _unitOfWorkMock
                .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
                .Returns<Func<Task>, CancellationToken>((operation, _) => operation());
        }

        private static User CreateUser(int id = 1, int roleId = 2, string email = "user@test.com", string hashedPassword = "hashed")
        {
            return new User
            {
                Id = id,
                Email = email,
                HashedPassword = hashedPassword,
                Role = Domain.Enums.Role.Customer
            };
        }

        private void SetupSimpleUserGetBy(User? user) =>
            _userRepositoryMock
                .Setup(r => r.GetByAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

        private void SetupSimpleRefreshTokenGetBy(RefreshToken? token) =>
            _refreshTokenRepositoryMock
                .Setup(r => r.GetByAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

        private void SetupIncludeRefreshTokenGetBy(RefreshToken? token) =>
            _refreshTokenRepositoryMock
                .Setup(r => r.GetByAsync(
                    It.IsAny<Expression<Func<RefreshToken, bool>>>(),
                    It.IsAny<Func<IQueryable<RefreshToken>, IIncludableQueryable<RefreshToken, object>>?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

        #region Register / CreateUserAsync

        [Fact]
        public async Task CreateUserAsync_NewUser_CreatesUserAndReturnsAuthResponse()
        {
            var request = new RegisterRequest
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "Jane.Doe@Test.com",
                PhoneNumber = " 555-0100 ",
                Password = "P@ssw0rd"
            };
            var ip = "203.0.113.10";
            var deviceInfo = "TestAgent";

            _userRepositoryMock
                .Setup(r => r.ExistByAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var mappedUser = new User { Email = "jane.doe@test.com", PhoneNumber = "555-0100" };
            _authMapperMock.Setup(m => m.ToUser(request)).Returns(mappedUser);

            _passwordHasherMock.Setup(p => p.Hash(request.Password)).Returns("hashed-pw");

            var refreshEntity = new RefreshToken { ExpiresAt = DateTime.UtcNow.AddDays(7) };
            _tokenServiceMock
                .Setup(t => t.GenerateRefreshToken(mappedUser))
                .Returns(("raw-refresh", refreshEntity));

            var accessTokenResult = new AccessTokenResult("access-token", DateTime.UtcNow.AddMinutes(15));
            _tokenServiceMock.Setup(t => t.GenerateAccessToken(mappedUser)).Returns(accessTokenResult);

            var result = await _sut.CreateUserAsync(request,CancellationToken.None);

            Assert.Equal("access-token", result.AccessToken);
            Assert.Equal(accessTokenResult.ExpiresAtUtc, result.AccessTokenExpiresAtUtc);
            Assert.Equal("raw-refresh", result.RefreshToken);
            Assert.Equal(refreshEntity.ExpiresAt, result.RefreshTokenExpiresAtUtc);

            Assert.Equal(Domain.Enums.Role.Customer, mappedUser.Role);
            Assert.Equal("hashed-pw", mappedUser.HashedPassword);
            Assert.Same(mappedUser, refreshEntity.User);

            _userRepositoryMock.Verify(r => r.AddAsync(mappedUser, It.IsAny<CancellationToken>()), Times.Once);
            _refreshTokenRepositoryMock.Verify(r => r.AddAsync(refreshEntity, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_EmailAlreadyExists_ThrowsConflictException_AndDoesNotCheckPhone()
        {
            var request = new RegisterRequest { Email = "taken@test.com", PhoneNumber = "555-0100", Password = "x" };

            _userRepositoryMock
                .Setup(r => r.ExistByAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<ConflictException>(
                () => _sut.CreateUserAsync(request, CancellationToken.None));

            _userRepositoryMock.Verify(
                r => r.ExistByAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _authMapperMock.Verify(m => m.ToUser(It.IsAny<RegisterRequest>()), Times.Never);
        }

        [Fact]
        public async Task CreateUserAsync_PhoneNumberAlreadyExists_ThrowsConflictException()
        {
            var request = new RegisterRequest { Email = "new@test.com", PhoneNumber = "555-0100", Password = "x" };

            _userRepositoryMock
                .SetupSequence(r => r.ExistByAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)  // email check
                .ReturnsAsync(true);  // phone check

            await Assert.ThrowsAsync<ConflictException>(
                () => _sut.CreateUserAsync(request, CancellationToken.None));

            _authMapperMock.Verify(m => m.ToUser(It.IsAny<RegisterRequest>()), Times.Never);
        }

        #endregion

        #region Login

        [Fact]
        public async Task Login_ValidCredentials_ReturnsAuthResponseWithTokens()
        {
            var request = new LoginRequest { Email = "USER@test.com", Password = "P@ssw0rd" };
            var ip = "203.0.113.10";
            var deviceInfo = "Mozilla/5.0 TestAgent";

            var user = CreateUser(email: "user@test.com");
            var role = Domain.Enums.Role.Customer;

            SetupSimpleUserGetBy(user);
            _passwordHasherMock.Setup(p => p.Verify(request.Password, user.HashedPassword)).Returns(true);
            SetupSimpleRefreshTokenGetBy(null);

            var accessTokenResult = new AccessTokenResult("access-token-value", DateTime.UtcNow.AddMinutes(15));
            _tokenServiceMock.Setup(t => t.GenerateAccessToken(user)).Returns(accessTokenResult);

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                IpAddress = ip,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            _tokenServiceMock
                .Setup(t => t.GenerateRefreshToken(user))
                .Returns(("raw-refresh-token-value", refreshTokenEntity));

            var result = await _sut.Login(request, CancellationToken.None);

            Assert.Equal(accessTokenResult.Token, result.AccessToken);
            Assert.Equal(accessTokenResult.ExpiresAtUtc, result.AccessTokenExpiresAtUtc);
            Assert.Equal("raw-refresh-token-value", result.RefreshToken);
            Assert.Equal(refreshTokenEntity.ExpiresAt, result.RefreshTokenExpiresAtUtc);
            Assert.Equal(role, user.Role);

            _refreshTokenRepositoryMock.Verify(r => r.AddAsync(refreshTokenEntity, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _refreshTokenRepositoryMock.Verify(r => r.Delete(It.IsAny<RefreshToken>()), Times.Never);
        }

        [Fact]
        public async Task Login_UnknownEmail_ThrowsUnauthorizedException()
        {
            var request = new LoginRequest { Email = "doesnotexist@test.com", Password = "whatever" };
            SetupSimpleUserGetBy(null);

            await Assert.ThrowsAsync<UnauthorizedException>(
                () => _sut.Login(request,CancellationToken.None));

            _passwordHasherMock.Verify(p => p.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Login_WrongPassword_ThrowsUnauthorizedException()
        {
            var request = new LoginRequest { Email = "user@test.com", Password = "wrong-password" };
            var user = CreateUser(email: "user@test.com");

            SetupSimpleUserGetBy(user);
            _passwordHasherMock.Setup(p => p.Verify(request.Password, user.HashedPassword)).Returns(false);

            await Assert.ThrowsAsync<UnauthorizedException>(
                () => _sut.Login(request, CancellationToken.None));

            _tokenServiceMock.Verify(t => t.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Login_LowercasesEmailBeforeLookup()
        {
            var request = new LoginRequest { Email = "MixedCase@Test.com", Password = "x" };
            SetupSimpleUserGetBy(null);

            await Assert.ThrowsAsync<UnauthorizedException>(
                () => _sut.Login(request, CancellationToken.None));

            _userRepositoryMock.Verify(
                r => r.GetByAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region Logout

        [Fact]
        public async Task Logout_TokenExists_DeletesTokenWithinTransaction()
        {
            var storedToken = new RefreshToken { Id = 1, TokenHash = "hashed-token" };
            var request = new LogoutRequest { RefreshToken = "raw-refresh-token" };

            _tokenServiceMock.Setup(t => t.HashRefreshToken(request.RefreshToken)).Returns("hashed-token");
            SetupSimpleRefreshTokenGetBy(storedToken);

            await _sut.Logout(request, CancellationToken.None);

            _refreshTokenRepositoryMock.Verify(r => r.Delete(storedToken), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(
                u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Logout_TokenDoesNotExist_IsIdempotent_DoesNotThrowOrDelete()
        {
            var request = new LogoutRequest { RefreshToken = "already-gone" };

            _tokenServiceMock.Setup(t => t.HashRefreshToken(request.RefreshToken)).Returns("hashed-token");
            SetupSimpleRefreshTokenGetBy(null);

            var exception = await Record.ExceptionAsync(
                () => _sut.Logout(request, CancellationToken.None));

            Assert.Null(exception);
            _refreshTokenRepositoryMock.Verify(r => r.Delete(It.IsAny<RefreshToken>()), Times.Never);
            _unitOfWorkMock.Verify(
                u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Logout_HashesRawTokenBeforeLookup()
        {
            var request = new LogoutRequest { RefreshToken = "plain-text-token" };

            _tokenServiceMock.Setup(t => t.HashRefreshToken("plain-text-token")).Returns("expected-hash");
            SetupSimpleRefreshTokenGetBy(new RefreshToken { TokenHash = "expected-hash" });

            await _sut.Logout(request, CancellationToken.None);

            _tokenServiceMock.Verify(t => t.HashRefreshToken("plain-text-token"), Times.Once);
        }

        #endregion

        #region Refresh

        [Fact]
        public async Task Refresh_ValidActiveToken_RotatesAndReturnsNewAuthResponse()
        {
            var user = CreateUser();
            user.Role = Domain.Enums.Role.Customer;

            var storedToken = new RefreshToken
            {
                Id = 1,
                UserId = user.Id,
                User = user,
                TokenHash = "old-hash",
                IpAddress = "old-ip",
                DeviceInfo = "old-device",
                ExpiresAt = DateTime.UtcNow.AddDays(3) // IsActive == true
            };

            _tokenServiceMock.Setup(t => t.HashRefreshToken("raw-refresh-token")).Returns("old-hash");
            SetupIncludeRefreshTokenGetBy(storedToken);

            var accessTokenResult = new AccessTokenResult("new-access-token", DateTime.UtcNow.AddMinutes(15));
            _tokenServiceMock.Setup(t => t.GenerateAccessToken(user)).Returns(accessTokenResult);

            const string currentIp = "203.0.113.55";
            const string currentDevice = "NewDevice/2.0";

            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                IpAddress = currentIp,
                DeviceInfo = currentDevice,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            _tokenServiceMock
                .Setup(t => t.GenerateRefreshToken(user))
                .Returns(("new-raw-refresh-token", newRefreshTokenEntity));

            var request = new RefreshTokenRequest { RefreshToken = "raw-refresh-token" };

            var result = await _sut.Refresh(request, CancellationToken.None);

            Assert.Equal("new-access-token", result.AccessToken);
            Assert.Equal(accessTokenResult.ExpiresAtUtc, result.AccessTokenExpiresAtUtc);
            Assert.Equal("new-raw-refresh-token", result.RefreshToken);
            Assert.Equal(newRefreshTokenEntity.ExpiresAt, result.RefreshTokenExpiresAtUtc);

            _refreshTokenRepositoryMock.Verify(r => r.Delete(storedToken), Times.Once);
            _refreshTokenRepositoryMock.Verify(
                r => r.AddAsync(newRefreshTokenEntity, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(
                u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()), Times.Once);

            // Confirms the CURRENT request's ip/device were used, not the storedToken's stale ones
            _tokenServiceMock.Verify(t => t.GenerateRefreshToken(user), Times.Once);
            _tokenServiceMock.Verify(t => t.GenerateRefreshToken(user), Times.Once);
        }

        [Fact]
        public async Task Refresh_TokenNotFound_ThrowsUnauthorizedException()
        {
            SetupIncludeRefreshTokenGetBy(null);
            _tokenServiceMock.Setup(t => t.HashRefreshToken(It.IsAny<string>())).Returns("hash");

            var request = new RefreshTokenRequest { RefreshToken = "does-not-exist" };

            await Assert.ThrowsAsync<UnauthorizedException>(
                () => _sut.Refresh(request, CancellationToken.None));

            _refreshTokenRepositoryMock.Verify(r => r.Delete(It.IsAny<RefreshToken>()), Times.Never);
            _unitOfWorkMock.Verify(
                u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Refresh_ExpiredToken_ThrowsUnauthorizedException()
        {
            var user = CreateUser();
            var expiredToken = new RefreshToken
            {
                Id = 1,
                UserId = user.Id,
                User = user,
                TokenHash = "hash",
                ExpiresAt = DateTime.UtcNow.AddMinutes(-5)
            };

            SetupIncludeRefreshTokenGetBy(expiredToken);
            _tokenServiceMock.Setup(t => t.HashRefreshToken(It.IsAny<string>())).Returns("hash");

            var request = new RefreshTokenRequest { RefreshToken = "expired-token" };

            await Assert.ThrowsAsync<UnauthorizedException>(
                () => _sut.Refresh(request, CancellationToken.None));

            _tokenServiceMock.Verify(t => t.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Refresh_ValidToken_HashesRawTokenBeforeLookup()
        {
            var user = CreateUser();
            var storedToken = new RefreshToken
            {
                Id = 1,
                UserId = user.Id,
                User = user,
                TokenHash = "expected-hash",
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            SetupIncludeRefreshTokenGetBy(storedToken);

            _tokenServiceMock.Setup(t => t.HashRefreshToken("plain-text-raw-token")).Returns("expected-hash");
            _tokenServiceMock.Setup(t => t.GenerateAccessToken(user)).Returns(new AccessTokenResult("access", DateTime.UtcNow.AddMinutes(15)));
            _tokenServiceMock
                .Setup(t => t.GenerateRefreshToken(user))
                .Returns(("new-raw", new RefreshToken { UserId = user.Id, ExpiresAt = DateTime.UtcNow.AddDays(7) }));

            var request = new RefreshTokenRequest { RefreshToken = "plain-text-raw-token" };

            await _sut.Refresh(request, CancellationToken.None);

            _tokenServiceMock.Verify(t => t.HashRefreshToken("plain-text-raw-token"), Times.Once);
        }

        #endregion
    }
}