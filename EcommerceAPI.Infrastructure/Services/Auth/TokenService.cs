using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EcommerceAPI.Infrastructure.Services.Auth
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;

        public TokenService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }
        /// <inheritdoc />
        public AccessTokenResult GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Guid.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: creds
            );

            return new AccessTokenResult(new JwtSecurityTokenHandler().WriteToken(token), DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes));
        }
        /// <inheritdoc />
        public (string RawToken, RefreshToken Entity) GenerateRefreshToken(User user)
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var rawToken = Convert.ToBase64String(randomBytes);

            var entity = new RefreshToken
            {
                TokenHash = Hash(rawToken),
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                UserId = user.Id
            };

            return (rawToken, entity);
        }

        /// <summary>
        /// Hashes the raw token using SHA256 and returns the Base64 encoded string.
        /// </summary>
        /// <param name="rawToken">The raw token to hash.</param>
        /// <returns>The Base64 encoded hash of the token.</returns>
        public string Hash(string rawToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToBase64String(bytes);
        }

        public bool Verify(string rawToken, string hashedToken)
        {
            var hashedRawToken = Hash(rawToken);
            return hashedRawToken == hashedToken;
        }

        public (string RawToken, VerificationToken Entity) GenerateActivationToken(User user)
        {
            byte[] randomBytes = new byte[4];
            RandomNumberGenerator.Fill(randomBytes);
            uint randomValue = BitConverter.ToUInt32(randomBytes, 0);
            int code = (int)(randomValue % 1_000_000);
            string rawToken = code.ToString("D6");

            string hashedToken= Hash(rawToken);
            var entity = new VerificationToken
            {
                TokenHash = hashedToken,
                Purpose = VerificationPurpose.EmailVerification,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                User = user
            };

            return (rawToken, entity);
        }

    }
}
