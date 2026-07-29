using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Domain.Entities;


namespace EcommerceAPI.Application.UseCases.Auth.Login
{
    public class LoginUseCase : ILoginUseCase
    {
        private readonly IRepository<User> _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IRepository<RefreshToken> _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Role> _roleRepository;
        public LoginUseCase(IRepository<User> userRepository, IPasswordHasher passwordHasher,
            ITokenService tokenService, IRepository<RefreshToken> refreshTokenRepository,
            IUnitOfWork unitOfWork, IRepository<Role> roleRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _roleRepository = roleRepository;
        }
        public async Task<AuthResponse> Handle(LoginRequest request,string ipAdress, string deviceInfo)
        {
            var user = await _userRepository.GetByAsync(u => u.Email == request.Email.ToLower()) 
                ?? throw new UnauthorizedException("Invalid credentials");
            if (!_passwordHasher.Verify(request.Password, user.HashedPassword))
                throw new UnauthorizedException("Invalid credentials");
            user.Role = await _roleRepository.GetByAsync(r => r.Id == user.RoleId)??
                throw new NotFoundException("Role not found");
            var accesstoken = _tokenService.GenerateAccessToken(user);
            var existingRefreshToken = await _refreshTokenRepository
                .GetByAsync(rt => rt.UserId == user.Id && rt.IpAddress == ipAdress);
            if(null != existingRefreshToken)
            {
                _refreshTokenRepository.Delete(existingRefreshToken);
            }
            var refreshToken = _tokenService.GenerateRefreshToken(user, ipAdress, deviceInfo);

            await _refreshTokenRepository.AddAsync(refreshToken.Entity);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accesstoken.Token,
                AccessTokenExpiresAtUtc = accesstoken.ExpiresAtUtc,
                RefreshToken = refreshToken.RawToken,
                RefreshTokenExpiresAtUtc = refreshToken.Entity.ExpiresAt
            };
        }
    }
}
