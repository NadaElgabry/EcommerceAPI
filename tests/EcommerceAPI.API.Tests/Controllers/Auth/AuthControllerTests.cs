using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.UseCases.Auth.Login;
using EcommerceAPI.Application.UseCases.Auth.Logout;
using EcommerceAPI.Application.UseCases.Auth.Refresh;
using EcommerceAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EcommerceAPI.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<ILoginUseCase> _loginUseCaseMock = new();
        private readonly Mock<IRefreshUseCase> _refreshUseCaseMock = new();
        private readonly Mock<ILogoutUseCase> _logoutUseCaseMock = new();
        private readonly Mock<IAuthService> _authServiceMock = new();
        private readonly AuthController _sut;

        public AuthControllerTests()
        {
            _sut = new AuthController(_loginUseCaseMock.Object, _authServiceMock.Object, _refreshUseCaseMock.Object, _logoutUseCaseMock.Object);
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

        [Fact]
        public async Task Login_ValidRequest_ReturnsOkWithAuthResponse()
        {
            // Arrange
            var request = new LoginRequest { Email = "user@test.com", Password = "P@ssw0rd" };
            var expectedResponse = new AuthResponse
            {
                AccessToken = "access-token",
                AccessTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(15),
                RefreshToken = "refresh-token",
                RefreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(7)
            };

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContext("203.0.113.10", "Mozilla/5.0 TestAgent")
            };

            _loginUseCaseMock
                .Setup(l => l.Login(request, "203.0.113.10", "Mozilla/5.0 TestAgent", It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _sut.Login(request, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var response = Assert.IsType<AuthResponse>(okResult.Value);
            Assert.Equal(expectedResponse.AccessToken, response.AccessToken);
            Assert.Equal(expectedResponse.RefreshToken, response.RefreshToken);
        }

        [Fact]
        public async Task Login_NoRemoteIpAddress_PassesUnknownAsIp()
        {
            // Arrange: RemoteIpAddress is null (e.g. some test/hosting scenarios)
            var request = new LoginRequest { Email = "user@test.com", Password = "P@ssw0rd" };

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContext(remoteIp: null, userAgent: "TestAgent/1.0")
            };

            _loginUseCaseMock
                .Setup(l => l.Login(request, "unknown", "TestAgent/1.0", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AuthResponse
                {
                    AccessToken = "access-token",
                    AccessTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(15),
                    RefreshToken = "refresh-token",
                    RefreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(7)
                });

            // Act
            var actionResult = await _sut.Login(request, CancellationToken.None);

            // Assert
            Assert.IsType<OkObjectResult>(actionResult);
            _loginUseCaseMock.Verify(
                l => l.Login(request, "unknown", "TestAgent/1.0", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Login_ExtractsUserAgentHeader_AsDeviceInfo()
        {
            // Arrange
            var request = new LoginRequest { Email = "user@test.com", Password = "P@ssw0rd" };
            const string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) TestBrowser/9.9";

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContext("198.51.100.7", userAgent)
            };

            _loginUseCaseMock
                .Setup(l => l.Login(request, "198.51.100.7", userAgent, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AuthResponse
                {
                    AccessToken = "access-token",
                    AccessTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(15),
                    RefreshToken = "refresh-token",
                    RefreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(7)
                });

            // Act
            await _sut.Login(request, CancellationToken.None);

            // Assert
            _loginUseCaseMock.Verify(
                l => l.Login(request, "198.51.100.7", userAgent, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Login_UseCaseThrows_ExceptionPropagatesToExceptionMiddleware()
        {
            // Arrange: verifies the controller does not swallow exceptions itself.
            // Actual HTTP status mapping (401/404/etc.) should be verified in an
            // integration test against your global exception handling middleware.
            var request = new LoginRequest { Email = "user@test.com", Password = "wrong" };

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContext("127.0.0.1", "TestAgent")
            };

            _loginUseCaseMock
                .Setup(l => l.Login(request, "127.0.0.1", "TestAgent", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("simulated failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Login(request, CancellationToken.None));
        }
    }
}