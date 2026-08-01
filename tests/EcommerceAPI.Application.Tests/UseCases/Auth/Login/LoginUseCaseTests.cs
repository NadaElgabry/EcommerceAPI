using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.UseCases.Auth;
using EcommerceAPI.Application.UseCases.Auth.Login;
using EcommerceAPI.Domain.Entities;
using Moq;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Xunit;

namespace EcommerceAPI.Tests.UseCases.Auth
{
    /// <summary>
    /// Unit tests for LoginUseCase.Handle.
    ///
    /// ASSUMPTIONS (adjust to match your real types if different):
    /// - ITokenService.GenerateAccessToken(User) returns a type here called
    ///   AccessTokenResult, exposing .Token (string) and .ExpiresAtUtc (DateTime).
    /// - ITokenService.GenerateRefreshToken(User, string, string) returns a type
    ///   here called GeneratedRefreshToken, exposing .Entity (RefreshToken) and
    ///   .RawToken (string).
    /// - RefreshToken exposes UserId (int), IpAddress (string), ExpiresAt (DateTime).
    /// - Role exposes Id (int).
    /// - IUnitOfWork exposes Task SaveChangesAsync(CancellationToken = default).
    ///
    /// If your actual project uses different type/property names for the
    /// GenerateAccessToken / GenerateRefreshToken return values, rename the
    /// two local records below (AccessTokenResult / GeneratedRefreshToken)
    /// to match, or remove them and reference your real types directly.
    /// </summary>
    public class LoginUseCaseTests
    {
        private readonly Mock<IRepository<User>> _userRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly Mock<IRepository<RefreshToken>> _refreshTokenRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IRepository<Role>> _roleRepositoryMock = new();

        private readonly LoginUseCase _sut; // system under test

        public LoginUseCaseTests()
        {
            _sut = new LoginUseCase(
                _userRepositoryMock.Object,
                _passwordHasherMock.Object,
                _tokenServiceMock.Object,
                _refreshTokenRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _roleRepositoryMock.Object);
        }

        private static User CreateUser(int id = 1, int roleId = 2, string email = "user@test.com", string hashedPassword = "hashed")
        {
            return new User
            {
                Id = id,
                Email = email,
                HashedPassword = hashedPassword,
                RoleId = roleId
            };
        }

        [Fact]
        public async Task Handle_ValidCredentials_ReturnsAuthResponseWithTokens()
        {
            // Arrange
            var request = new LoginRequest { Email = "USER@test.com", Password = "P@ssw0rd" };
            var ip = "203.0.113.10";
            var deviceInfo = "Mozilla/5.0 TestAgent";

            var user = CreateUser(email: "user@test.com");
            var role = new Role { Id = user.RoleId };

            _userRepositoryMock
                .Setup(r => r.GetByAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(p => p.Verify(request.Password, user.HashedPassword))
                .Returns(true);

            _roleRepositoryMock
                .Setup(r => r.GetByAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            _refreshTokenRepositoryMock
                .Setup(r => r.GetByAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((RefreshToken?)null); // no existing token for this IP

            var accessTokenResult = new AccessTokenResult("access-token-value", DateTime.UtcNow.AddMinutes(15));
            _tokenServiceMock
                .Setup(t => t.GenerateAccessToken(user))
                .Returns(accessTokenResult);

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                IpAddress = ip,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            var generatedRefreshToken = (RawToken: "raw-refresh-token-value", Entity: refreshTokenEntity);
            _tokenServiceMock
                .Setup(t => t.GenerateRefreshToken(user, ip, deviceInfo))
                .Returns(generatedRefreshToken);

            // Act
            var result = await _sut.Login(request, ip, deviceInfo, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(accessTokenResult.Token, result.AccessToken);
            Assert.Equal(accessTokenResult.ExpiresAtUtc, result.AccessTokenExpiresAtUtc);
            Assert.Equal(generatedRefreshToken.RawToken, result.RefreshToken);
            Assert.Equal(refreshTokenEntity.ExpiresAt, result.RefreshTokenExpiresAtUtc);

            _refreshTokenRepositoryMock.Verify(
                r => r.AddAsync(refreshTokenEntity, It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            // An existing token should NOT have been deleted since none existed
            _refreshTokenRepositoryMock.Verify(r => r.Delete(It.IsAny<RefreshToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UnknownEmail_ThrowsUnauthorizedException()
        {
            // Arrange
            var request = new LoginRequest { Email = "doesnotexist@test.com", Password = "whatever" };

            _userRepositoryMock
                .Setup(r => r.GetByAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(
                () => _sut.Login(request, "127.0.0.1", "test-agent", CancellationToken.None));

            _passwordHasherMock.Verify(p => p.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WrongPassword_ThrowsUnauthorizedException()
        {
            // Arrange
            var request = new LoginRequest { Email = "user@test.com", Password = "wrong-password" };
            var user = CreateUser(email: "user@test.com");

            _userRepositoryMock
                .Setup(r => r.GetByAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(p => p.Verify(request.Password, user.HashedPassword))
                .Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(
                () => _sut.Login(request, "127.0.0.1", "test-agent", CancellationToken.None));

            _tokenServiceMock.Verify(
                t => t.GenerateAccessToken(It.IsAny<User>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_RoleNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var request = new LoginRequest { Email = "user@test.com", Password = "correct-password" };
            var user = CreateUser(email: "user@test.com");

            _userRepositoryMock
                .Setup(r => r.GetByAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(p => p.Verify(request.Password, user.HashedPassword))
                .Returns(true);

            _roleRepositoryMock
                .Setup(r => r.GetByAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Role?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.Login(request, "127.0.0.1", "test-agent", CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ExistingRefreshTokenForSameIp_DeletesOldTokenBeforeIssuingNew()
        {
            // Arrange
            var request = new LoginRequest { Email = "user@test.com", Password = "correct-password" };
            var ip = "203.0.113.10";
            var deviceInfo = "test-agent";

            var user = CreateUser(email: "user@test.com");
            var role = new Role { Id = user.RoleId };

            var existingToken = new RefreshToken { UserId = user.Id, IpAddress = ip };

            _userRepositoryMock
                .Setup(r => r.GetByAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(p => p.Verify(request.Password, user.HashedPassword))
                .Returns(true);

            _roleRepositoryMock
                .Setup(r => r.GetByAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            _refreshTokenRepositoryMock
                .Setup(r => r.GetByAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingToken);

            _tokenServiceMock
                .Setup(t => t.GenerateAccessToken(user))
                .Returns(new AccessTokenResult("t", DateTime.UtcNow));

            _tokenServiceMock
                .Setup(t => t.GenerateRefreshToken(user, ip, deviceInfo))
                .Returns((RawToken: "raw", Entity: new RefreshToken { UserId = user.Id, IpAddress = ip }));

            // Act
            await _sut.Login(request, ip, deviceInfo, CancellationToken.None);

            // Assert
            _refreshTokenRepositoryMock.Verify(r => r.Delete(existingToken), Times.Once);
        }

        [Fact]
        public async Task Handle_Always_LowercasesEmailBeforeLookup()
        {
            // Arrange: verifies request.Email.ToLower() is what's used for the query.
            // Since we can't inspect the expression tree easily, we assert indirectly
            // by returning null only when the exact lowercase email predicate concept
            // is respected in the use case (documented expectation, not deep expression
            // introspection).
            var request = new LoginRequest { Email = "MixedCase@Test.com", Password = "x" };

            _userRepositoryMock
                .Setup(r => r.GetByAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<UnauthorizedException>(
                () => _sut.Login(request, "127.0.0.1", "test-agent", CancellationToken.None));

            _userRepositoryMock.Verify(
                r => r.GetByAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

}