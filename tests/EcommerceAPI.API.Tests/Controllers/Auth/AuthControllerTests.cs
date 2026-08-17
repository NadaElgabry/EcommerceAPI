using System.Net;
using System.Reflection;
using System.Security.Claims;
using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces.Iservices;
using EcommerceAPI.Application.UseCases.Auth.Login;
using EcommerceAPI.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EcommerceAPI.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<ILoginUseCase> _loginUseCaseMock = new();
        private readonly Mock<IAuthService> _authServiceMock = new();
        private readonly AuthController _sut;

        public AuthControllerTests()
        {
            _sut = new AuthController(
                _loginUseCaseMock.Object,
                _authServiceMock.Object
            );
        }

        private static DefaultHttpContext BuildHttpContext(
            string? remoteIp,
            string userAgent)
        {
            var context = new DefaultHttpContext();

            if (remoteIp is not null)
            {
                context.Connection.RemoteIpAddress =
                    IPAddress.Parse(remoteIp);
            }

            context.Request.Headers["User-Agent"] = userAgent;

            return context;
        }

        [Fact]
        public async Task Login_ValidRequest_ReturnsOkWithAuthResponse()
        {
            var request = new LoginRequest
            {
                Email = "user@test.com",
                Password = "P@ssw0rd"
            };

            var expectedResponse = new AuthResponse
            {
                AccessToken = "access-token",
                AccessTokenExpiresAtUtc =
                    DateTime.UtcNow.AddMinutes(15),
                RefreshToken = "refresh-token",
                RefreshTokenExpiresAtUtc =
                    DateTime.UtcNow.AddDays(7)
            };

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContext(
                    "203.0.113.10",
                    "Mozilla/5.0 TestAgent"
                )
            };

            _loginUseCaseMock
                .Setup(useCase => useCase.Login(
                    request,
                    "203.0.113.10",
                    "Mozilla/5.0 TestAgent",
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(expectedResponse);

            var actionResult = await _sut.Login(
                request,
                CancellationToken.None
            );

            var okResult =
                Assert.IsType<OkObjectResult>(actionResult);

            var response =
                Assert.IsType<AuthResponse>(okResult.Value);

            Assert.Equal(
                expectedResponse.AccessToken,
                response.AccessToken
            );

            Assert.Equal(
                expectedResponse.RefreshToken,
                response.RefreshToken
            );
        }

        [Fact]
        public async Task Login_NoRemoteIpAddress_PassesUnknownAsIp()
        {
            var request = new LoginRequest
            {
                Email = "user@test.com",
                Password = "P@ssw0rd"
            };

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContext(
                    remoteIp: null,
                    userAgent: "TestAgent/1.0"
                )
            };

            _loginUseCaseMock
                .Setup(useCase => useCase.Login(
                    request,
                    "unknown",
                    "TestAgent/1.0",
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(new AuthResponse
                {
                    AccessToken = "access-token",
                    AccessTokenExpiresAtUtc =
                        DateTime.UtcNow.AddMinutes(15),
                    RefreshToken = "refresh-token",
                    RefreshTokenExpiresAtUtc =
                        DateTime.UtcNow.AddDays(7)
                });

            var actionResult = await _sut.Login(
                request,
                CancellationToken.None
            );

            Assert.IsType<OkObjectResult>(actionResult);

            _loginUseCaseMock.Verify(
                useCase => useCase.Login(
                    request,
                    "unknown",
                    "TestAgent/1.0",
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task Login_ExtractsUserAgentHeader_AsDeviceInfo()
        {
            var request = new LoginRequest
            {
                Email = "user@test.com",
                Password = "P@ssw0rd"
            };

            const string userAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) TestBrowser/9.9";

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContext(
                    "198.51.100.7",
                    userAgent
                )
            };

            _loginUseCaseMock
                .Setup(useCase => useCase.Login(
                    request,
                    "198.51.100.7",
                    userAgent,
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(new AuthResponse
                {
                    AccessToken = "access-token",
                    AccessTokenExpiresAtUtc =
                        DateTime.UtcNow.AddMinutes(15),
                    RefreshToken = "refresh-token",
                    RefreshTokenExpiresAtUtc =
                        DateTime.UtcNow.AddDays(7)
                });

            await _sut.Login(
                request,
                CancellationToken.None
            );

            _loginUseCaseMock.Verify(
                useCase => useCase.Login(
                    request,
                    "198.51.100.7",
                    userAgent,
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task Login_UseCaseThrows_ExceptionPropagatesToExceptionMiddleware()
        {
            var request = new LoginRequest
            {
                Email = "user@test.com",
                Password = "wrong"
            };

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContext(
                    "127.0.0.1",
                    "TestAgent"
                )
            };

            _loginUseCaseMock
                .Setup(useCase => useCase.Login(
                    request,
                    "127.0.0.1",
                    "TestAgent",
                    It.IsAny<CancellationToken>()
                ))
                .ThrowsAsync(
                    new InvalidOperationException(
                        "simulated failure"
                    )
                );

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.Login(
                    request,
                    CancellationToken.None
                )
            );
        }

        [Fact]
        public async Task ResetPassword_AuthenticatedSubject_PassesGuidAndReturnsNoContent()
        {
            var userGuid = Guid.NewGuid();
            var request = CreateResetPasswordRequest();

            var cancellationToken =
                new CancellationTokenSource().Token;

            var context = BuildHttpContext(
                "127.0.0.1",
                "TestAgent"
            );

            context.User =
                new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                            new Claim(
                                ClaimTypes.NameIdentifier,
                                userGuid.ToString()
                            )
                        },
                        "TestAuthentication"
                    )
                );

            _sut.ControllerContext =
                new ControllerContext
                {
                    HttpContext = context
                };

            var result = await _sut.ResetPassword(
                request,
                cancellationToken
            );

            Assert.IsType<NoContentResult>(result);

            _authServiceMock.Verify(
                service => service.ResetPasswordAsync(
                    userGuid,
                    request,
                    cancellationToken
                ),
                Times.Once
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("not-a-guid")]
        public async Task ResetPassword_MissingOrMalformedSubject_ThrowsUnauthorizedException(
            string? subject)
        {
            var context = BuildHttpContext(
                "127.0.0.1",
                "TestAgent"
            );

            var claims =
                subject is null
                    ? Array.Empty<Claim>()
                    : new[]
                    {
                        new Claim(
                            ClaimTypes.NameIdentifier,
                            subject
                        )
                    };

            context.User =
                new ClaimsPrincipal(
                    new ClaimsIdentity(
                        claims,
                        "TestAuthentication"
                    )
                );

            _sut.ControllerContext =
                new ControllerContext
                {
                    HttpContext = context
                };

            await Assert.ThrowsAsync<UnauthorizedException>(
                () => _sut.ResetPassword(
                    CreateResetPasswordRequest(),
                    CancellationToken.None
                )
            );

            _authServiceMock.Verify(
                service => service.ResetPasswordAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<ResetPasswordRequest>(),
                    It.IsAny<CancellationToken>()
                ),
                Times.Never
            );
        }

        [Fact]
        public void ResetPassword_HasAuthorizeAttribute()
        {
            var method =
                typeof(AuthController)
                    .GetMethod(
                        nameof(AuthController.ResetPassword)
                    );

            Assert.NotNull(method);

            Assert.NotNull(
                method.GetCustomAttribute<AuthorizeAttribute>()
            );
        }

        private static ResetPasswordRequest CreateResetPasswordRequest()
        {
            return new ResetPasswordRequest
            {
                OldPassword = "OldP@ssword1",
                NewPassword = "NewP@ssword2",
                ConfirmNewPassword = "NewP@ssword2"
            };
        }
    }
}