using FluentValidation;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Application.Mappers.Mappings;
using EcommerceAPI.Application.Services.Auth;
using EcommerceAPI.Application.Services.UserService;
using EcommerceAPI.Application.Services.TagService;
using EcommerceAPI.Application.Validators;
using Microsoft.Extensions.DependencyInjection;
using EcommerceAPI.Application.Services.CategoryService;
using EcommerceAPI.Application.Services.ProductService;

namespace EcommerceAPI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthMapper, AuthMapper>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IUserMapper, UserMapper>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductMapper,ProductMapper>();
        services.AddScoped<ICategoryMapper, CategoryMapper>();
        services.AddScoped<ITagMapper, TagMapper>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IUserActivityService, UserActivityService>();
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateProfileRequestValidator>();

        return services;
    }
}