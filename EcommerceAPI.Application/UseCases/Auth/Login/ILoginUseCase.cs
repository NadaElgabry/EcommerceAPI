using EcommerceAPI.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.UseCases.Auth.Login
{
    public interface ILoginUseCase
    {
        Task<AuthResponse> Handle(LoginRequest request, string ipAddress, string deviceInfo);
    }
}
