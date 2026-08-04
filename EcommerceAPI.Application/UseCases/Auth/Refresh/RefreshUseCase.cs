using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Application.UseCases.Auth.Refresh
{
    public class RefreshUseCase : IRefreshUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IRepository<RefreshToken> _refreshTokenRepository;

        public RefreshUseCase(IUnitOfWork unitOfWork, ITokenService tokenService, IRepository<RefreshToken> refreshTokenRepository)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;

        }
        public async Task<AuthResponse> Refresh(RefreshTokenRequest request, string ipAddress, string deviceInfo, CancellationToken cancellationToken = default)
        {
            var hashedToken = _tokenService.HashRefreshToken(request.RefreshToken);

            var storedToken = await _refreshTokenRepository.GetByAsync(
                predicate: rt => rt.TokenHash == hashedToken,
                include: query => query.Include(rt => rt.User)
                                       .ThenInclude(u => u.Role));
            
            if (storedToken == null || !storedToken.IsActive)
            {
                throw new UnauthorizedException("Invalid or expired refresh token.");
            }

            var accessTokenResult = _tokenService.GenerateAccessToken(storedToken.User);

            var (rawToken, newRefreshToken) = _tokenService.GenerateRefreshToken(storedToken.User, ipAddress, deviceInfo);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                _refreshTokenRepository.Delete(storedToken);
                await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);


            return new AuthResponse
            {
                AccessToken = accessTokenResult.Token,
                AccessTokenExpiresAtUtc = accessTokenResult.ExpiresAtUtc,
                RefreshToken = rawToken,
                RefreshTokenExpiresAtUtc = newRefreshToken.ExpiresAt
            };
        }
    }
}
