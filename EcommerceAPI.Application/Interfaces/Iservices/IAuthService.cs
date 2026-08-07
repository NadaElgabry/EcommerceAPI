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
        /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
        /// <returns>The authentication response containing access and refresh tokens</returns>
        Task<AuthResponse> CreateUserAsync(
            RegisterRequest request,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Logs in a user with the provided credentials and
        /// returns an authentication response containing access and refresh tokens.
        /// </summary>
        /// <param name="request">request entity contains the user's login credentials</param>
        /// <param name="cancellationToken">a token to monitor for cancellation requests</param>
        /// <returns>an authentication response containing access and refresh tokens</returns>
        /// <exception cref="UnauthorizedException">Thrown when the provided credentials are invalid.</exception>
        /// <exception cref="NotFoundException">Thrown when the user or their associated role is not found.</exception>
        public Task<AuthResponse> Login(
            LoginRequest request, CancellationToken cancellationToken);
        
        /// <summary>
        /// Refreshes the access token using the provided refresh token.
        /// </summary>
        /// <param name="request">The request containing the refresh token.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response containing the new access token.</returns>
        public Task<AuthResponse> Refresh(
            RefreshTokenRequest request, CancellationToken cancellationToken = default);


        /// <summary>
        /// Executes the logout use case.
        /// </summary>
        /// <param name="request">The logout request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task Logout(
            LogoutRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Receives an Email address and sends a reset code to the provided email address.
        /// </summary>
        /// <param name="request">The request containing the user's email address.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task<String> ForgotPasswordAsync(
            ForgotPasswordRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifies the reset code provided by the user.
        /// </summary>
        /// <param name="request">The request containing the reset code.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task<VerifyResetCodeResponse> VerifyResetCodeAsync(
            VerifyResetCodeRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resets the user's password using the provided reset code and new password.
        /// </summary>
        /// <param name="request">The request containing the reset code and new password.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task ResetPasswordAsync(
            ResetPasswordRequest request, CancellationToken cancellationToken = default);

    }
}