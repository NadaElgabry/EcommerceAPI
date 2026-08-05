using EcommerceAPI.Application.DTOs.Auth;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface IAuthService
    {
        /// <summary>
        /// Creates a new user with the provided registration details and
        /// returns an authentication response containing access and refresh tokens.
        /// </summary>
        /// <param name="request">The registration request containing user details</param>
        /// <param name="ipAddress">The IP address of the client making the request</param>
        /// <param name="deviceInfo">Information about the device making the request</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
        /// <returns>The authentication response containing access and refresh tokens</returns>
        Task<AuthResponse> CreateUserAsync(
            RegisterRequest request, string ipAddress, string deviceInfo,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Logs in a user with the provided credentials and
        /// returns an authentication response containing access and refresh tokens.
        /// </summary>
        /// <param name="request">request entity contains the user's login credentials</param>
        /// <param name="ipAddress">the IP address of the client making the request</param>
        /// <param name="deviceInfo">information about the device making the request</param>
        /// <param name="cancellationToken">a token to monitor for cancellation requests</param>
        /// <returns>an authentication response containing access and refresh tokens</returns>
        /// <exception cref="UnauthorizedException">Thrown when the provided credentials are invalid.</exception>
        /// <exception cref="NotFoundException">Thrown when the user or their associated role is not found.</exception>
        public Task<AuthResponse> Login(
            LoginRequest request, string ipAddress, string deviceInfo, CancellationToken cancellationToken);
        
        /// <summary>
        /// Refreshes the access token using the provided refresh token.
        /// </summary>
        /// <param name="request">The request containing the refresh token.</param>
        /// <param name="ipAddress">The IP address of the client making the request.</param>
        /// <param name="deviceInfo">The device information of the client making the request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response containing the new access token.</returns>
        public Task<AuthResponse> Refresh(
            RefreshTokenRequest request, string ipAddress,
            string deviceInfo, CancellationToken cancellationToken = default);


        /// <summary>
        /// Executes the logout use case.
        /// </summary>
        /// <param name="request">The logout request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Logout(
            LogoutRequest request, CancellationToken cancellationToken = default);
    }
}