using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EcommerceAPI.Tests.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="AuthController"/>. The controller depends only on
    /// <see cref="IAuthService"/> and exposes Login, Register, Refresh, and Logout.
    /// </summary>
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock = new();
        private readonly AuthController _sut;

        public AuthControllerTests()
        {
            _sut = new AuthController(_authServiceMock.Object);
        }

        private static DefaultHttpContext BuildHttpContext(string? remoteIp, string userAgent)
        {
            var context = new DefaultHttpContext();

            if (remoteIp is not null)
            {
                context.Connection.RemoteIpAddress = IPAddress.Parse(remoteIp);
            }

            context.Request.Headers["User-Agent"] = userAgent;
            return context;
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

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContext("203.0.113.10", "Mozilla/5.0 TestAgent")
            };

            _authServiceMock
                .Setup(s => s.Login(request, "203.0.113.10", "Mozilla/5.0 TestAgent", It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var actionResult = await _sut.Login(request, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var response = Assert.IsType<AuthResponse>(okResult.Value);
            Assert.Equal(expectedResponse.AccessToken, response.AccessToken);
            Assert.Equal(expectedResponse.RefreshToken, response.RefreshToken);
        }

        [Fact]
        public async Task Login_NoRemoteIpAddress_PassesUnknownAsIp()
        {
            var request = new LoginRequest { Email = "user@test.com", Password = "P@ssw0rd" };

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContext(remoteIp: null, userAgent: "TestAgent/1.0")
            };

            _authServiceMock
                .Setup(s => s.Login(request, "unknown", "TestAgent/1.0", It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildAuthResponse());

            var actionResult = await _sut.Login(request, CancellationToken.None);

            Assert.IsType<OkObjectResult>(actionResult);
            _authServiceMock.Verify(
                s => s.Login(request, "unknown", "TestAgent/1.0", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Login_ExtractsUserAgentHeader_AsDeviceInfo()
        {
            var request = new LoginRequest { Email = "user@test.com", Password = "P@ssw0rd" };
            const string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) TestBrowser/9.9";

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContext("198.51.100.7", userAgent)
            };

            _authServiceMock
                .Setup(s => s.Login(request, "198.51.100.7", userAgent, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildAuthResponse());

            await _sut.Login(request, CancellationToken.None);

            _authServiceMock.Verify(
                s => s.Login(request, "198.51.100.7", userAgent, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Login_ServiceThrows_ExceptionPropagatesToExceptionMiddleware()
        {
            // Verifies the controller does not swallow exceptions itself; the actual
            // HTTP status mapping (401/404/409/etc.) belongs in an integration test
            // against your global exception handling middleware.
            var request = new LoginRequest { Email = "user@test.com", Password = "wrong" };

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContext("127.0.0.1", "TestAgent")
            };

            _authServiceMock
                .Setup(s => s.Login(request, "127.0.0.1", "TestAgent", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("simulated failure"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Login(request, CancellationToken.None));
        }

        #endregion

        #region Register

        [Fact]
        public async Task Register_ValidRequest_ReturnsOkWithAuthResponse()
        {
            var request = new RegisterRequest
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@test.com",
                PhoneNumber = "555-0100",
                Password = "P@ssw0rd"
            };
            var expectedResponse = BuildAuthResponse();

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContext("203.0.113.10", "Mozilla/5.0 TestAgent")
            };

            _authServiceMock
                .Setup(s => s.CreateUserAsync(request, "203.0.113.10", "Mozilla/5.0 TestAgent", It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var actionResult = await _sut.Register(request, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<AuthResponse>(okResult.Value);
            Assert.Equal(expectedResponse.AccessToken, response.AccessToken);
            Assert.Equal(expectedResponse.RefreshToken, response.RefreshToken);
        }

        [Fact]
        public async Task Register_NoRemoteIpAddress_PassesUnknownAsIp()
        {
            var request = new RegisterRequest { Email = "jane@test.com", PhoneNumber = "555-0100", Password = "x" };

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContext(remoteIp: null, userAgent: "TestAgent/1.0")
            };

            _authServiceMock
                .Setup(s => s.CreateUserAsync(request, "unknown", "TestAgent/1.0", It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildAuthResponse());

            await _sut.Register(request, CancellationToken.None);

            _authServiceMock.Verify(
                s => s.CreateUserAsync(request, "unknown", "TestAgent/1.0", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Register_ServiceThrowsConflict_ExceptionPropagatesToExceptionMiddleware()
        {
            var request = new RegisterRequest { Email = "taken@test.com", PhoneNumber = "555-0100", Password = "x" };

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContext("127.0.0.1", "TestAgent")
            };

            _authServiceMock
                .Setup(s => s.CreateUserAsync(request, "127.0.0.1", "TestAgent", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ConflictException("A user with this email already exists."));

            await Assert.ThrowsAsync<ConflictException>(() => _sut.Register(request, CancellationToken.None));
        }

        #endregion

        #region Refresh

        [Fact]
        public async Task Refresh_ValidRequest_ReturnsOkWithAuthResponse()
        {
            var request = new RefreshTokenRequest { RefreshToken = "raw-refresh-token" };
            var expectedResponse = BuildAuthResponse();

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContext("203.0.113.10", "Mozilla/5.0 TestAgent")
            };

            _authServiceMock
                .Setup(s => s.Refresh(request, "203.0.113.10", "Mozilla/5.0 TestAgent", It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var actionResult = await _sut.Refresh(request, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var response = Assert.IsType<AuthResponse>(okResult.Value);
            Assert.Equal(expectedResponse.RefreshToken, response.RefreshToken);
        }

        [Fact]
        public async Task Refresh_NoRemoteIpAddress_PassesUnknownAsIp()
        {
            var request = new RefreshTokenRequest { RefreshToken = "raw-refresh-token" };

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContext(remoteIp: null, userAgent: "TestAgent/1.0")
            };

            _authServiceMock
                .Setup(s => s.Refresh(request, "unknown", "TestAgent/1.0", It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildAuthResponse());

            await _sut.Refresh(request, CancellationToken.None);

            _authServiceMock.Verify(
                s => s.Refresh(request, "unknown", "TestAgent/1.0", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Refresh_ServiceThrowsUnauthorized_ExceptionPropagatesToExceptionMiddleware()
        {
            var request = new RefreshTokenRequest { RefreshToken = "invalid" };

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContext("127.0.0.1", "TestAgent")
            };

            _authServiceMock
                .Setup(s => s.Refresh(request, "127.0.0.1", "TestAgent", It.IsAny<CancellationToken>()))
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