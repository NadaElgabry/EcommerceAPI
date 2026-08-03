using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Interfaces.Auth
{
    public record AccessTokenResult(
        string Token,
        DateTime ExpiresAtUtc
    );

    public interface ITokenService
    {
        AccessTokenResult GenerateAccessToken(User user);

        (string RawToken, RefreshToken Entity)
            GenerateRefreshToken();
    }
}