using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.UseCases.Auth.ResetPassword
{
    public class ResetPasswordUseCase : IResetPasswordUseCase
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public ResetPasswordUseCase(
            IRepository<User> userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task ResetPasswordAsync(
            Guid userGuid,
            ResetPasswordRequest request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByAsync(
                user => user.Guid == userGuid,
                cancellationToken
            );

            if (user is null)
            {
                throw new NotFoundException("User not found.");
            }

            var oldPasswordIsCorrect = _passwordHasher.Verify(
                request.OldPassword,
                user.HashedPassword
            );

            if (!oldPasswordIsCorrect)
            {
                throw new UnauthorizedException(
                    "Invalid old password."
                );
            }

            var newPasswordIsSameAsOld = _passwordHasher.Verify(
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
                _passwordHasher.Hash(request.NewPassword);

            user.UpdatedAt = now;

            _userRepository.Update(user);

            var activeRefreshTokens =
                await _refreshTokenRepository.GetActiveByUserIdAsync(
                    user.Id,
                    cancellationToken
                );

            foreach (var refreshToken in activeRefreshTokens)
            {
                refreshToken.RevokedAt = now;
                _refreshTokenRepository.Update(refreshToken);
            }

            await _unitOfWork.SaveChangesAsync(
                cancellationToken
            );
        }
    }
}