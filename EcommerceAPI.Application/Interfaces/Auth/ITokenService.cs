using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Domain.Enums;

namespace EcommerceAPI.Application.Interfaces.Auth
{
    public record AccessTokenResult(
        string Token,
        DateTime ExpiresAtUtc
    );

    public interface ITokenService
    {
        /// <summary>
        /// Generates an access token for the specified user.
        /// </summary>
        /// <param name="user">The user for whom to generate the access token.</param>
        /// <returns>The generated access token.</returns>
        public AccessTokenResult GenerateAccessToken(User user);

        /// <summary>
        /// Generates a refresh token for the specified user, along with the associated entity.
        /// </summary>
        /// <param name="user">The user for whom to generate the refresh token.</param>
        /// <returns>A tuple containing the raw token and the associated refresh token entity.</returns>
        public (string RawToken, RefreshToken Entity) GenerateRefreshToken(User user);

        /// <summary>
        /// Hashes the given token and returns the hashed value.
        /// </summary>
        /// <param name="Token">The token to hash.</param>
        /// <returns>The hashed token.</returns>
        public string Hash(string Token);

        /// <summary>
        /// Generates a high-entropy token for secure operations.
        /// </summary>
        /// <returns>The high-entropy token</returns>
        public string GenerateHighEntropyToken();

        /// <summary>
        /// Verifies the given raw token against the hashed token.
        /// </summary>
        /// <param name="rawToken">The raw token to verify.</param>
        /// <param name="hashedToken">The hashed token to compare against.</param>
        /// <returns>Whether the tokens match or not.</returns>
        public bool Verify(string rawToken, string hashedToken);

        /// <summary>
        /// Generates a verification token for the specified user and purpose,
        /// returning both the raw token and the associated entity.
        /// </summary>
        /// <param name="user">The user for whom to generate the verification token.</param>
        /// <param name="purpose">The purpose of the verification token.</param>
        /// <returns>A tuple containing the raw token and the associated verification token entity.</returns>
        public (string RawToken, VerificationToken Entity) GenerateVerificationToken(User user, VerificationPurpose purpose);

    }
}