using EcommerceAPI.Application.DTOs.Auth;

namespace EcommerceAPI.Application.UseCases.Auth.ResetPassword
{
    public interface IResetPasswordUseCase
    {
        Task ResetPasswordAsync(
            Guid userGuid,
            ResetPasswordRequest request,
            CancellationToken cancellationToken
        );
    }
}