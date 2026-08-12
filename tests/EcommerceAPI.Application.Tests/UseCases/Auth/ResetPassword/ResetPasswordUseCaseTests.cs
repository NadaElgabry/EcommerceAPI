using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.UseCases.Auth.ResetPassword;
using EcommerceAPI.Domain.Entities;
using Moq;
using Xunit;

namespace EcommerceAPI.Tests.UseCases.Auth.ResetPassword
{
    public class ResetPasswordUseCaseTests
    {
        private readonly Mock<IRepository<User>> _userRepositoryMock = new();
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        private readonly ResetPasswordUseCase _sut;

        public ResetPasswordUseCaseTests()
        {
            _refreshTokenRepositoryMock
                .Setup(repository =>
                    repository.GetActiveByUserIdAsync(
                        It.IsAny<int>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RefreshToken>());

            _sut = new ResetPasswordUseCase(
                _userRepositoryMock.Object,
                _refreshTokenRepositoryMock.Object,
                _passwordHasherMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task ResetPasswordAsync_ValidRequest_UpdatesPasswordAndSaves()
        {
            var userGuid = Guid.NewGuid();
            var user = CreateUser(userGuid);
            var request = CreateRequest();

            SetupUser(user);

            SetupPasswordChecks(
                request,
                user,
                oldPasswordMatches: true,
                newPasswordMatches: false
            );

            _passwordHasherMock
                .Setup(hasher => hasher.Hash(request.NewPassword))
                .Returns("new-hash");

            await _sut.ResetPasswordAsync(
                userGuid,
                request,
                CancellationToken.None
            );

            Assert.Equal("new-hash", user.HashedPassword);
            Assert.NotNull(user.UpdatedAt);

            _userRepositoryMock.Verify(
                repository => repository.Update(user),
                Times.Once
            );

            _unitOfWorkMock.Verify(
                unitOfWork =>
                    unitOfWork.SaveChangesAsync(
                        CancellationToken.None
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task ResetPasswordAsync_ValidRequest_RevokesActiveRefreshTokens()
        {
            var user = CreateUser(Guid.NewGuid());
            var request = CreateRequest();

            var refreshToken1 = new RefreshToken
            {
                Id = 1,
                UserId = user.Id,
                TokenHash = "token-1",
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            var refreshToken2 = new RefreshToken
            {
                Id = 2,
                UserId = user.Id,
                TokenHash = "token-2",
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            SetupUser(user);

            SetupPasswordChecks(
                request,
                user,
                oldPasswordMatches: true,
                newPasswordMatches: false
            );

            _passwordHasherMock
                .Setup(hasher => hasher.Hash(request.NewPassword))
                .Returns("new-hash");

            _refreshTokenRepositoryMock
                .Setup(repository =>
                    repository.GetActiveByUserIdAsync(
                        user.Id,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new List<RefreshToken>
                    {
                        refreshToken1,
                        refreshToken2
                    }
                );

            await _sut.ResetPasswordAsync(
                user.Guid,
                request,
                CancellationToken.None
            );

            Assert.NotNull(refreshToken1.RevokedAt);
            Assert.NotNull(refreshToken2.RevokedAt);

            _refreshTokenRepositoryMock.Verify(
                repository => repository.Update(refreshToken1),
                Times.Once
            );

            _refreshTokenRepositoryMock.Verify(
                repository => repository.Update(refreshToken2),
                Times.Once
            );

            _unitOfWorkMock.Verify(
                unitOfWork =>
                    unitOfWork.SaveChangesAsync(
                        CancellationToken.None
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task ResetPasswordAsync_WrongOldPassword_ThrowsUnauthorizedAndDoesNotSave()
        {
            var user = CreateUser(Guid.NewGuid());
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
                    CancellationToken.None
                )
            );

            VerifyNothingSaved();
        }

        [Fact]
        public async Task ResetPasswordAsync_AuthenticatedUserNotFound_ThrowsNotFoundAndDoesNotSave()
        {
            _userRepositoryMock
                .Setup(repository =>
                    repository.GetByAsync(
                        It.IsAny<Expression<Func<User, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.ResetPasswordAsync(
                    Guid.NewGuid(),
                    CreateRequest(),
                    CancellationToken.None
                )
            );

            VerifyNothingSaved();
        }

        [Fact]
        public async Task ResetPasswordAsync_NewPasswordEqualsOldPassword_ThrowsBadRequestAndDoesNotSave()
        {
            var user = CreateUser(Guid.NewGuid());
            var request = CreateRequest();

            SetupUser(user);

            SetupPasswordChecks(
                request,
                user,
                oldPasswordMatches: true,
                newPasswordMatches: true
            );

            await Assert.ThrowsAsync<BadRequestException>(
                () => _sut.ResetPasswordAsync(
                    user.Guid,
                    request,
                    CancellationToken.None
                )
            );

            VerifyNothingSaved();
        }

        [Fact]
        public void ResetPasswordRequest_MismatchedConfirmation_FailsValidation()
        {
            var request = CreateRequest();

            request.ConfirmNewPassword =
                "DifferentP@ssword3";

            var results = Validate(request);

            Assert.Contains(
                results,
                result =>
                    result.MemberNames.Contains(
                        nameof(
                            ResetPasswordRequest.ConfirmNewPassword
                        )
                    )
            );
        }

        [Theory]
        [InlineData("Short1!")]
        [InlineData("NoNumber!")]
        [InlineData("NoSpecial123")]
        public void ResetPasswordRequest_WeakNewPassword_FailsValidation(
            string newPassword)
        {
            var request = CreateRequest();

            request.NewPassword = newPassword;
            request.ConfirmNewPassword = newPassword;

            var results = Validate(request);

            Assert.Contains(
                results,
                result =>
                    result.MemberNames.Contains(
                        nameof(
                            ResetPasswordRequest.NewPassword
                        )
                    )
            );
        }

        [Fact]
        public async Task ResetPasswordAsync_HashesNewPassword()
        {
            var user = CreateUser(Guid.NewGuid());
            var request = CreateRequest();

            SetupUser(user);

            SetupPasswordChecks(
                request,
                user,
                oldPasswordMatches: true,
                newPasswordMatches: false
            );

            _passwordHasherMock
                .Setup(hasher =>
                    hasher.Hash(request.NewPassword))
                .Returns("new-hash");

            await _sut.ResetPasswordAsync(
                user.Guid,
                request,
                CancellationToken.None
            );

            _passwordHasherMock.Verify(
                hasher =>
                    hasher.Hash(request.NewPassword),
                Times.Once
            );

            _passwordHasherMock.Verify(
                hasher =>
                    hasher.Hash(request.OldPassword),
                Times.Never
            );
        }

        [Fact]
        public async Task ResetPasswordAsync_PropagatesCancellationToken()
        {
            var cancellationToken =
                new CancellationTokenSource().Token;

            var user = CreateUser(Guid.NewGuid());
            var request = CreateRequest();

            _userRepositoryMock
                .Setup(repository =>
                    repository.GetByAsync(
                        It.IsAny<Expression<Func<User, bool>>>(),
                        cancellationToken))
                .ReturnsAsync(user);

            SetupPasswordChecks(
                request,
                user,
                oldPasswordMatches: true,
                newPasswordMatches: false
            );

            _passwordHasherMock
                .Setup(hasher =>
                    hasher.Hash(request.NewPassword))
                .Returns("new-hash");

            await _sut.ResetPasswordAsync(
                user.Guid,
                request,
                cancellationToken
            );

            _userRepositoryMock.Verify(
                repository =>
                    repository.GetByAsync(
                        It.IsAny<Expression<Func<User, bool>>>(),
                        cancellationToken),
                Times.Once
            );

            _refreshTokenRepositoryMock.Verify(
                repository =>
                    repository.GetActiveByUserIdAsync(
                        user.Id,
                        cancellationToken),
                Times.Once
            );

            _unitOfWorkMock.Verify(
                unitOfWork =>
                    unitOfWork.SaveChangesAsync(
                        cancellationToken),
                Times.Once
            );
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

        private void SetupPasswordChecks(
            ResetPasswordRequest request,
            User user,
            bool oldPasswordMatches,
            bool newPasswordMatches)
        {
            _passwordHasherMock
                .Setup(hasher =>
                    hasher.Verify(
                        request.OldPassword,
                        user.HashedPassword))
                .Returns(oldPasswordMatches);

            _passwordHasherMock
                .Setup(hasher =>
                    hasher.Verify(
                        request.NewPassword,
                        user.HashedPassword))
                .Returns(newPasswordMatches);
        }

        private void VerifyNothingSaved()
        {
            _userRepositoryMock.Verify(
                repository =>
                    repository.Update(It.IsAny<User>()),
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

        private static User CreateUser(Guid guid)
        {
            return new User
            {
                Id = 1,
                Guid = guid,
                HashedPassword = "old-hash"
            };
        }

        private static ResetPasswordRequest CreateRequest()
        {
            return new ResetPasswordRequest
            {
                OldPassword = "OldP@ssword1",
                NewPassword = "NewP@ssword2",
                ConfirmNewPassword = "NewP@ssword2"
            };
        }

        private static List<ValidationResult> Validate(
            ResetPasswordRequest request)
        {
            var results =
                new List<ValidationResult>();

            Validator.TryValidateObject(
                request,
                new ValidationContext(request),
                results,
                validateAllProperties: true
            );

            return results;
        }
    }
}