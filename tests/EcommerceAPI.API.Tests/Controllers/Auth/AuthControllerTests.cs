using System;
using System.Threading;
using System.Threading.Tasks;
using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EcommerceAPI.Tests.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="AuthController"/>. The controller depends only on
    /// <see cref="IAuthService"/> and exposes Login, Register, ActivateAccount,
    /// IsEmailAvailable, Refresh, and Logout.
    ///
    /// NOTE: The controller does not currently extract client IP or User-Agent from
    /// HttpContext, and IAuthService does not accept device metadata. Tests that
    /// asserted on that behavior have been removed; re-add them if/when that
    /// functionality is implemented.
    /// </summary>
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock = new();
        private readonly AuthController _sut;

        public AuthControllerTests()
        {
            _sut = new AuthController(_authServiceMock.Object);
        }

        private static AuthResponse BuildAuthResponse() => new()
        {
            AccessToken = "access-token",
            AccessTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(15),
            RefreshToken = "refresh-token",
            RefreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        };

        #region Login

        [Fact]
        public async Task Login_ValidRequest_ReturnsOkWithAuthResponse()
        {
            var request = new LoginRequest { Email = "user@test.com", Password = "P@ssw0rd" };
            var expectedResponse = BuildAuthResponse();

            _authServiceMock
                .Setup(s => s.Login(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var actionResult = await _sut.Login(request, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var response = Assert.IsType<AuthResponse>(okResult.Value);
            Assert.Equal(expectedResponse.AccessToken, response.AccessToken);
            Assert.Equal(expectedResponse.RefreshToken, response.RefreshToken);
        }

        [Fact]
        public async Task Login_ServiceThrowsUnauthorized_ExceptionPropagatesToExceptionMiddleware()
        {
            // Verifies the controller does not swallow exceptions itself; the actual
            // HTTP status mapping (401/404/409/etc.) belongs in an integration test
            // against your global exception handling middleware.
            var request = new LoginRequest { Email = "user@test.com", Password = "wrong" };

            _authServiceMock
                .Setup(s => s.Login(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new UnauthorizedException("Invalid credentials"));

            await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.Login(request, CancellationToken.None));
        }

        #endregion

        #region Register

        [Fact]
        public async Task Register_ValidRequest_ReturnsOkWithActivationToken()
        {
            // CreateUserAsync currently returns the raw activation token (string),
            // not an AuthResponse. Update this test's assertions if/when Register
            // is changed to return a bool instead.
            var request = new RegisterRequest
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@test.com",
                PhoneNumber = "555-0100",
                Password = "P@ssw0rd"
            };
            const string expectedToken = "raw-activation-token";

            _authServiceMock
                .Setup(s => s.CreateUserAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedToken);

            var actionResult = await _sut.Register(request, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var token = Assert.IsType<string>(okResult.Value);
            Assert.Equal(expectedToken, token);
        }

        [Fact]
        public async Task Register_ServiceThrowsConflict_ExceptionPropagatesToExceptionMiddleware()
        {
            var request = new RegisterRequest
            {
                Email = "taken@test.com",
                PhoneNumber = "555-0100",
                Password = "x"
            };

            _authServiceMock
                .Setup(s => s.CreateUserAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ConflictException("A user with this email already exists."));

            await Assert.ThrowsAsync<ConflictException>(() => _sut.Register(request, CancellationToken.None));
        }

        #endregion

        #region ActivateAccount

        [Fact]
        public async Task ActivateAccount_ValidRequest_ReturnsOkWithAuthResponse()
        {
            var request = new ActivateEmailRequest { Token = "raw-activation-token" };
            var expectedResponse = BuildAuthResponse();

            _authServiceMock
                .Setup(s => s.ActivateEmailAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var actionResult = await _sut.ActivateAccount(request, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var response = Assert.IsType<AuthResponse>(okResult.Value);
            Assert.Equal(expectedResponse.AccessToken, response.AccessToken);
            Assert.Equal(expectedResponse.RefreshToken, response.RefreshToken);
        }

        [Fact]
        public async Task ActivateAccount_InvalidOrExpiredToken_ExceptionPropagatesToExceptionMiddleware()
        {
            var request = new ActivateEmailRequest { Token = "bad-token" };

            _authServiceMock
                .Setup(s => s.ActivateEmailAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException("Invalid activation token."));

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.ActivateAccount(request, CancellationToken.None));
        }

        #endregion

        #region IsEmailAvailable

        [Fact]
        public async Task IsEmailAvailable_EmailNotTaken_ReturnsOkTrue()
        {
            const string email = "available@test.com";

            _authServiceMock
                .Setup(s => s.IsEmailAvailable(
                    It.Is<EmailRequest>(r => r.Email == email),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var actionResult = await _sut.IsEmailAvailable(new EmailRequest { Email = email }, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task IsEmailAvailable_EmailTaken_ReturnsOkFalse()
        {
            const string email = "taken@test.com";

            _authServiceMock
                .Setup(s => s.IsEmailAvailable(new EmailRequest { Email = email }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var actionResult = await _sut.IsEmailAvailable(new EmailRequest { Email = email }, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.False((bool)okResult.Value!);
        }

        #endregion

        #region Refresh

        [Fact]
        public async Task Refresh_ValidRequest_ReturnsOkWithAuthResponse()
        {
            var request = new RefreshTokenRequest { RefreshToken = "raw-refresh-token" };
            var expectedResponse = BuildAuthResponse();

            _authServiceMock
                .Setup(s => s.Refresh(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var actionResult = await _sut.Refresh(request, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var response = Assert.IsType<AuthResponse>(okResult.Value);
            Assert.Equal(expectedResponse.RefreshToken, response.RefreshToken);
        }

        [Fact]
        public async Task Refresh_ServiceThrowsUnauthorized_ExceptionPropagatesToExceptionMiddleware()
        {
            var request = new RefreshTokenRequest { RefreshToken = "invalid" };

            _authServiceMock
                .Setup(s => s.Refresh(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new UnauthorizedException("Invalid or expired refresh token."));

            await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.Refresh(request, CancellationToken.None));
        }

        #endregion

        #region Logout

        [Fact]
        public async Task Logout_ValidRequest_ReturnsNoContent()
        {
            var request = new LogoutRequest { RefreshToken = "raw-refresh-token" };

            _authServiceMock
                .Setup(s => s.Logout(request, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var actionResult = await _sut.Logout(request, CancellationToken.None);

            Assert.IsType<NoContentResult>(actionResult);
            _authServiceMock.Verify(s => s.Logout(request, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Logout_NonExistentToken_StillReturnsNoContent_BecauseLogoutIsIdempotent()
        {
            // The service layer treats logging out a token that no longer exists as a
            // no-op success rather than an error, so the controller should reflect that.
            // From the controller's perspective this is the same code path as the
            // "valid request" case above (it never inspects service internals) — this
            // test documents the idempotency contract rather than exercising different
            // controller logic.
            var request = new LogoutRequest { RefreshToken = "already-gone" };

            _authServiceMock
                .Setup(s => s.Logout(request, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var actionResult = await _sut.Logout(request, CancellationToken.None);

            Assert.IsType<NoContentResult>(actionResult);
        }

        #endregion
    }
}