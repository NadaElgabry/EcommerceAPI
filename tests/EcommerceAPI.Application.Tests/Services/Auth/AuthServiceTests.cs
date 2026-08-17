using System.Linq.Expressions;
using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Services.Auth;
using EcommerceAPI.Application.Validators.Auth;
using EcommerceAPI.Domain.Entities;
using Moq;
using Xunit;

namespace EcommerceAPI.Tests.Services.Auth
{
    public class AuthServiceTests
    {
        private readonly Mock<IRepository<User>> _userRepositoryMock = new();
        private readonly Mock<IRepository<RefreshToken>> _refreshTokenRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            var validator = new ResetPasswordRequestValidator();

            _refreshTokenRepositoryMock
                .Setup(repository =>
                    repository.GetAllByAsync(
                        It.IsAny<Expression<Func<RefreshToken, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RefreshToken>());

            _sut = new AuthService(
                _userRepositoryMock.Object,
                _refreshTokenRepositoryMock.Object,
                _passwordHasherMock.Object,
                _unitOfWorkMock.Object,
                validator
            );
        }

        [Fact]
        public async Task ResetPasswordAsync_ValidRequest_UpdatesPasswordAndRevokesTokens()
        {
            var user = CreateUser();
            var request = CreateRequest();

            var refreshToken = new RefreshToken
            {
                Id = 1,
                UserId = user.Id,
                TokenHash = "token-hash",
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            SetupUser(user);

            _passwordHasherMock
                .Setup(hasher =>
                    hasher.Verify(
                        request.OldPassword,
                        user.HashedPassword))
                .Returns(true);

            _passwordHasherMock
                .Setup(hasher =>
                    hasher.Verify(
                        request.NewPassword,
                        user.HashedPassword))
                .Returns(false);

            _passwordHasherMock
                .Setup(hasher =>
                    hasher.Hash(request.NewPassword))
                .Returns("new-hash");

            _refreshTokenRepositoryMock
                .Setup(repository =>
                    repository.GetAllByAsync(
                        It.IsAny<Expression<Func<RefreshToken, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new List<RefreshToken>
                    {
                        refreshToken
                    });

            await _sut.ResetPasswordAsync(
                user.Guid,
                request,
                CancellationToken.None
            );

            Assert.Equal(
                "new-hash",
                user.HashedPassword
            );

            Assert.NotNull(user.UpdatedAt);
            Assert.NotNull(refreshToken.RevokedAt);

            _userRepositoryMock.Verify(
                repository =>
                    repository.Update(user),
                Times.Once
            );

            _refreshTokenRepositoryMock.Verify(
                repository =>
                    repository.Update(refreshToken),
                Times.Once
            );

            _unitOfWorkMock.Verify(
                unitOfWork =>
                    unitOfWork.SaveChangesAsync(
                        CancellationToken.None),
                Times.Once
            );
        }

        [Fact]
        public async Task ResetPasswordAsync_WrongOldPassword_ThrowsUnauthorized()
        {
            var user = CreateUser();
            var request = CreateRequest();

            SetupUser(user);

            _passwordHasherMock
                .Setup(hasher =>
                    hasher.Verify(
                        request.OldPassword,
                        user.HashedPassword))
                .Returns(false);

            await Assert.ThrowsAsync<UnauthorizedException>(
                () => _sut.ResetPasswordAsync(
                    user.Guid,
                    request,
                    CancellationToken.None)
            );

            VerifyNothingSaved();
        }

        [Fact]
        public async Task ResetPasswordAsync_UserNotFound_ThrowsNotFound()
        {
            var request = CreateRequest();

            _userRepositoryMock
                .Setup(repository =>
                    repository.GetByAsync(
                        It.IsAny<Expression<Func<User, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.ResetPasswordAsync(
                    Guid.NewGuid(),
                    request,
                    CancellationToken.None)
            );

            VerifyNothingSaved();
        }

        [Fact]
        public async Task ResetPasswordAsync_NewPasswordSameAsOld_ThrowsBadRequest()
        {
            var user = CreateUser();

            var request = new ResetPasswordRequest
            {
                OldPassword = "SamePassword1!",
                NewPassword = "SamePassword1!",
                ConfirmNewPassword = "SamePassword1!"
            };

            SetupUser(user);

            _passwordHasherMock
                .Setup(hasher =>
                    hasher.Verify(
                        request.OldPassword,
                        user.HashedPassword))
                .Returns(true);

            _passwordHasherMock
                .Setup(hasher =>
                    hasher.Verify(
                        request.NewPassword,
                        user.HashedPassword))
                .Returns(true);

            await Assert.ThrowsAsync<BadRequestException>(
                () => _sut.ResetPasswordAsync(
                    user.Guid,
                    request,
                    CancellationToken.None)
            );

            VerifyNothingSaved();
        }

        [Fact]
        public async Task ResetPasswordAsync_InvalidRequest_ThrowsValidationException()
        {
            var request = new ResetPasswordRequest
            {
                OldPassword = "",
                NewPassword = "short",
                ConfirmNewPassword = "different"
            };

            await Assert.ThrowsAsync<EcommerceAPI.Application.Exceptions.ValidationException>(
                () => _sut.ResetPasswordAsync(
                    Guid.NewGuid(),
                    request,
                    CancellationToken.None)
            );

            _userRepositoryMock.Verify(
                repository =>
                    repository.GetByAsync(
                        It.IsAny<Expression<Func<User, bool>>>(),
                        It.IsAny<CancellationToken>()),
                Times.Never
            );

            VerifyNothingSaved();
        }

        private void SetupUser(User user)
        {
            _userRepositoryMock
                .Setup(repository =>
                    repository.GetByAsync(
                        It.IsAny<Expression<Func<User, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
        }

        private void VerifyNothingSaved()
        {
            _userRepositoryMock.Verify(
                repository =>
                    repository.Update(
                        It.IsAny<User>()),
                Times.Never
            );

            _refreshTokenRepositoryMock.Verify(
                repository =>
                    repository.Update(
                        It.IsAny<RefreshToken>()),
                Times.Never
            );

            _unitOfWorkMock.Verify(
                unitOfWork =>
                    unitOfWork.SaveChangesAsync(
                        It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        private static User CreateUser()
        {
            return new User
            {
                Id = 1,
                Guid = Guid.NewGuid(),
                HashedPassword = "old-hash"
            };
        }

        private static ResetPasswordRequest CreateRequest()
        {
            return new ResetPasswordRequest
            {
                OldPassword = "OldPassword1!",
                NewPassword = "NewPassword2!",
                ConfirmNewPassword = "NewPassword2!"
            };
        }
    }
}