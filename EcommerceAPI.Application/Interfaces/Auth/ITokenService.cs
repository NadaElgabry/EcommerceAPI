using EcommerceAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.Auth
{
    public record AccessTokenResult(string Token, DateTime ExpiresAtUtc);

    public interface ITokenService
    {
        AccessTokenResult GenerateAccessToken(User user);
        string GenerateRefreshToken();
        string HashRefreshToken(string rawToken);
    }
}
