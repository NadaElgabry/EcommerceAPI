using EcommerceAPI.Application.DTOs.Auth;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface IAuthService
    {
        Task<AuthResponse> CreateUserAsync(
            RegisterRequest request, string ipAddress, string deviceInfo,
            CancellationToken cancellationToken = default
        );
    }
}