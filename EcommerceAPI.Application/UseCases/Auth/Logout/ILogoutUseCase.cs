using EcommerceAPI.Application.DTOs.Auth;

namespace EcommerceAPI.Application.UseCases.Auth.Logout
{
    public interface ILogoutUseCase
    {
        /// <summary>
        /// Executes the logout use case.
        /// </summary>
        /// <param name="request">The logout request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task ExecuteAsync(LogoutRequest request, CancellationToken cancellationToken = default);
    }
}
