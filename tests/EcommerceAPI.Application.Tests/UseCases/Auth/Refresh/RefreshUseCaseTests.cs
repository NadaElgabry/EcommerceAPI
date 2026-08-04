using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.UseCases.Auth.Refresh;
using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Linq.Expressions;

namespace EcommerceAPI.Tests.UseCases.Auth
{
    /// <summary>
    /// Unit tests for RefreshUseCase.Refresh.
    ///
    /// ASSUMPTIONS:
    /// - IUnitOfWork.ExecuteInTransactionAsync(Func&lt;Task&gt;, CancellationToken) exists.
    /// - IRepository&lt;RefreshToken&gt;.GetByAsync(predicate, include, cancellationToken)
    ///   matches the named-argument call style used in RefreshUseCase.
    /// </summary>
    public class RefreshUseCaseTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly Mock<IRepository<RefreshToken>> _refreshTokenRepositoryMock = new();

        private readonly RefreshUseCase _sut;

        public RefreshUseCaseTests()
        {
            _sut = new RefreshUseCase(
                _unitOfWorkMock.Object,
                _tokenServiceMock.Object,
                _refreshTokenRepositoryMock.Object);

            _unitOfWorkMock
                .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
                .Returns<Func<Task>, CancellationToken>((operation, _) => operation());
        }

        private static User CreateUser(int id = 1, int roleId = 2)
        {
            return new User
            {
                Id = id,
                Guid = Guid.NewGuid(),
                RoleId = roleId,
                Role = new Role { Id = roleId, Name = "Customer" }
            };
        }

        private void SetupGetByAsync(RefreshToken? tokenToReturn)
        {
            _refreshTokenRepositoryMock
                .Setup(r => r.GetByAsync(
                    It.IsAny<Expression<Func<RefreshToken, bool>>>(),
                    It.IsAny<Func<IQueryable<RefreshToken>, IIncludableQueryable<RefreshToken, object>>?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(tokenToReturn);
        }

        [Fact]
        public async Task Refresh_ValidActiveToken_RotatesAndReturnsNewAuthResponse()
        {
            // Arrange
            var user = CreateUser();
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

            SetupGetByAsync(storedToken);

            _tokenServiceMock
                .Setup(t => t.HashRefreshToken("raw-refresh-token"))
                .Returns("old-hash");

            var accessTokenResult = new AccessTokenResult("new-access-token", DateTime.UtcNow.AddMinutes(15));
            _tokenServiceMock
                .Setup(t => t.GenerateAccessToken(user))
                .Returns(accessTokenResult);

            // NEW request's ip/device — should be used for the new token, not the stored (old) ones
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
                .Setup(t => t.GenerateRefreshToken(user, currentIp, currentDevice))
                .Returns(("new-raw-refresh-token", newRefreshTokenEntity));

            var request = new RefreshTokenRequest { RefreshToken = "raw-refresh-token" };

            // Act
            var result = await _sut.Refresh(request, currentIp, currentDevice, CancellationToken.None);

            // Assert
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

            // Confirms the CURRENT request's ip/device were used, not storedToken's stale ones
            _tokenServiceMock.Verify(t => t.GenerateRefreshToken(user, currentIp, currentDevice), Times.Once);
            _tokenServiceMock.Verify(t => t.GenerateRefreshToken(user, "old-ip", "old-device"), Times.Never);
        }

        [Fact]
        public async Task Refresh_TokenNotFound_ThrowsUnauthorizedException()
        {
            SetupGetByAsync(null);
            _tokenServiceMock.Setup(t => t.HashRefreshToken(It.IsAny<string>())).Returns("hash");

            var request = new RefreshTokenRequest { RefreshToken = "does-not-exist" };

            await Assert.ThrowsAsync<UnauthorizedException>(
                () => _sut.Refresh(request, "1.2.3.4", "agent", CancellationToken.None));

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

            SetupGetByAsync(expiredToken);
            _tokenServiceMock.Setup(t => t.HashRefreshToken(It.IsAny<string>())).Returns("hash");

            var request = new RefreshTokenRequest { RefreshToken = "expired-token" };

            await Assert.ThrowsAsync<UnauthorizedException>(
                () => _sut.Refresh(request, "1.2.3.4", "agent", CancellationToken.None));

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

            SetupGetByAsync(storedToken);

            _tokenServiceMock
                .Setup(t => t.HashRefreshToken("plain-text-raw-token"))
                .Returns("expected-hash");

            _tokenServiceMock
                .Setup(t => t.GenerateAccessToken(user))
                .Returns(new AccessTokenResult("access", DateTime.UtcNow.AddMinutes(15)));

            _tokenServiceMock
                .Setup(t => t.GenerateRefreshToken(user, "1.2.3.4", "agent"))
                .Returns(("new-raw", new RefreshToken { UserId = user.Id, ExpiresAt = DateTime.UtcNow.AddDays(7) }));

            var request = new RefreshTokenRequest { RefreshToken = "plain-text-raw-token" };

            await _sut.Refresh(request, "1.2.3.4", "agent", CancellationToken.None);

            _tokenServiceMock.Verify(t => t.HashRefreshToken("plain-text-raw-token"), Times.Once);
        }
    }
}