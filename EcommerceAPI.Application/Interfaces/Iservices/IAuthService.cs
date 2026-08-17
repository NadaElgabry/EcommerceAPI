using EcommerceAPI.Application.DTOs.Auth;

namespace EcommerceAPI.Application.Interfaces.Iservices
{
    public interface IAuthService
    {
        Task ResetPasswordAsync(
            Guid userGuid,
            ResetPasswordRequest request,
            CancellationToken cancellationToken
        );
    }
}