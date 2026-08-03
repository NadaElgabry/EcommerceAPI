using EcommerceAPI.Application.DTOs.Auth;

namespace EcommerceAPI.Application.UseCases.Auth.Refresh
{
    public interface IRefreshUseCase
    {
        /// <summary>
        /// Refreshes the access token using the provided refresh token.
        /// </summary>
        /// <param name="request">The request containing the refresh token.</param>
        /// <param name="ipAddress">The IP address of the client making the request.</param>
        /// <param name="deviceInfo">The device information of the client making the request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response containing the new access token.</returns>
        public Task<AuthResponse> ExecuteAsync(RefreshTokenRequest request, string ipAddress, string deviceInfo, CancellationToken cancellationToken = default);

    }
}
