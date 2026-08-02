using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.UseCases.Auth
{
    public class LogoutUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
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
                return;
            }

            _refreshTokenRepository.Delete(storedToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    } 
}
