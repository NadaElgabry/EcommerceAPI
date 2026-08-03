using EcommerceAPI.Application.DTOs.Auth;

namespace EcommerceAPI.Application.Interfaces.Iservices
{
    public interface IUserService
    {
        Task<AuthResponse> CreateUserAsync(
            RegisterRequest request,
            CancellationToken cancellationToken = default
        );
    }
}