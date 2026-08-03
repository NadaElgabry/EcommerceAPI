using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Application.UseCases.Auth.Logout
{
    public class LogoutUseCase : ILogoutUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LogoutUseCase> _logger;
        private readonly ITokenService _tokenService;
        private readonly IRepository<RefreshToken> _refreshTokenRepository;

        public LogoutUseCase(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IRepository<RefreshToken> refreshTokenRepository)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;
        }


        public async Task ExecuteAsync(LogoutRequest request, CancellationToken cancellationToken = default)
        {
            var hashedToken = _tokenService.HashRefreshToken(request.RefreshToken);

            var storedToken = await _refreshTokenRepository.GetByAsync(
                rt => rt.TokenHash == hashedToken,
                cancellationToken);

            if (storedToken == null)
            {

                // Logout is idempotent by design
                _logger.LogInformation(
                "Logout requested for a refresh token that no longer exists (already logged out, expired, or invalid). TokenHashPrefix: {TokenHashPrefix}",
                hashedToken[..Math.Min(8, hashedToken.Length)]);
                return;
            }

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                _refreshTokenRepository.Delete(storedToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);

        }
    } 
}
