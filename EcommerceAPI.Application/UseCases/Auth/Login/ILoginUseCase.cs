using EcommerceAPI.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.UseCases.Auth.Login
{
    public interface ILoginUseCase
    {
        /// <summary>
        /// Logs in a user with the provided credentials and returns an authentication response containing access and refresh tokens.
        /// </summary>
        /// <param name="request">request entity contains the user's login credentials</param>
        /// <param name="ipAdress">the IP address of the client making the request</param>
        /// <param name="deviceInfo">information about the device making the request</param>
        /// <returns>an authentication response containing access and refresh tokens</returns>
        /// <exception cref="UnauthorizedException">Thrown when the provided credentials are invalid.</exception>
        /// <exception cref="NotFoundException">Thrown when the user or their associated role is not found.</exception>
        public Task<AuthResponse> Login(LoginRequest request, string ipAddress, string deviceInfo);
    }
}
