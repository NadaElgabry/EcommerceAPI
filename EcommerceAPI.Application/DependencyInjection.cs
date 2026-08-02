using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.UseCases.Auth.Login;
using Microsoft.Extensions.DependencyInjection;

namespace EcommerceAPI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ILoginUseCase, LoginUseCase>();


        return services;
    }
}