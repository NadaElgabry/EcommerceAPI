using EcommerceAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.Auth
{
    public record AccessTokenResult(string Token, DateTime ExpiresAtUtc);

    public interface ITokenService
    {
        public AccessTokenResult GenerateAccessToken(User user);
        public (string RawToken, RefreshToken Entity) GenerateRefreshToken();
    }
}
