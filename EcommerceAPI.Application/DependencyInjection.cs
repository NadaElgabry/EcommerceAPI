using FluentValidation;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Iservices;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Application.Mappers.Mappings;
using EcommerceAPI.Application.Services.Auth;
using EcommerceAPI.Application.Services.UserService;
using EcommerceAPI.Application.Validators;
using Microsoft.Extensions.DependencyInjection;
using EcommerceAPI.Application.Services.UserService;
using EcommerceAPI.Application.Interfaces.Iservices;

namespace EcommerceAPI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuthMapper, AuthMapper>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserMapper, UserMapper>();
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();


        return services;
    }
}