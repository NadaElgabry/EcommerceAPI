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
        public AccessTokenResult GenerateServiceToken(string clientId, IEnumerable<string> scopes);
        public string Hash(string refreshToken);

        public string GenerateHighEntropyToken();

        public bool Verify(string rawToken, string hashedToken);

        public (string RawToken, VerificationToken Entity) GenerateVerificationToken(User user, VerificationPurpose purpose);

    }
}