namespace EcommerceAPI.Application.DTOs.Auth
{
    public class AuthResponse
    {
        public required string AccessToken { get; init; }
        public required DateTime AccessTokenExpiresAtUtc { get; init; }
        public required string RefreshToken { get; init; }
        public required DateTime RefreshTokenExpiresAtUtc { get; init; }
    }
}
