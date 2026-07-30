using EcommerceAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.Auth
{
    public record AccessTokenResult(string Token, DateTime ExpiresAtUtc);

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
        /// <param name="ipAdress">The IP address of the client.</param>
        /// <param name="deviceInfo">Information about the client device.</param>
        /// <returns>A tuple containing the raw token and the associated refresh token entity.</returns>

        public (string RawToken, RefreshToken Entity) GenerateRefreshToken(User user, string ipAdress, string deviceInfo);
    }
}
