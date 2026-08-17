using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Iservices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Domain.Entities;
using FluentValidation;

namespace EcommerceAPI.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<RefreshToken> _refreshTokenRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;

        public AuthService(
            IRepository<User> userRepository,
            IRepository<RefreshToken> refreshTokenRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork,
            IValidator<ResetPasswordRequest> resetPasswordValidator)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
            _resetPasswordValidator = resetPasswordValidator;
        }

        public async Task ResetPasswordAsync(
            Guid userGuid,
            ResetPasswordRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult =
                await _resetPasswordValidator.ValidateAsync(
                    request,
                    cancellationToken
                );

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group
                            .Select(error => error.ErrorMessage)
                            .ToArray()
                    );

                throw new EcommerceAPI.Application.Exceptions.ValidationException(
                    errors
                );
            }

            var user = await _userRepository.GetByAsync(
                user => user.Guid == userGuid,
                cancellationToken
            );

            if (user is null)
            {
                throw new NotFoundException(
                    "User not found."
                );
            }

            var oldPasswordIsCorrect =
                _passwordHasher.Verify(
                    request.OldPassword,
                    user.HashedPassword
                );

            if (!oldPasswordIsCorrect)
            {
                throw new UnauthorizedException(
                    "Invalid old password."
                );
            }

            var newPasswordIsSameAsOld =
                _passwordHasher.Verify(
                    request.NewPassword,
                    user.HashedPassword
                );

            if (newPasswordIsSameAsOld)
            {
                throw new BadRequestException(
                    "New password must be different from the old password."
                );
            }

            var now = DateTime.UtcNow;

            user.HashedPassword =
                _passwordHasher.Hash(
                    request.NewPassword
                );

            user.UpdatedAt = now;

            _userRepository.Update(user);

            var activeRefreshTokens =
                await _refreshTokenRepository.GetAllByAsync(
                    token =>
                        token.UserId == user.Id &&
                        token.RevokedAt == null &&
                        token.ExpiresAt > now,
                    cancellationToken
                );

            foreach (var refreshToken in activeRefreshTokens)
            {
                refreshToken.RevokedAt = now;

                _refreshTokenRepository.Update(
                    refreshToken
                );
            }

            await _unitOfWork.SaveChangesAsync(
                cancellationToken
            );
        }
    }
}