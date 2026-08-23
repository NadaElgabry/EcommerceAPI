using EcommerceAPI.Application.DTOs.Product;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Application.Mappers.Mappings;
using EcommerceAPI.Application.Services.Auth;
using EcommerceAPI.Application.Services.CategoryService;
using EcommerceAPI.Application.Services.ProductService;
using EcommerceAPI.Application.Services.TagService;
using EcommerceAPI.Application.Services.UserService;
using EcommerceAPI.Application.Validators;
using EcommerceAPI.Application.Validators.Product;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddValidatorsFromAssemblyContaining<ProductQueryParamsRequestValidator>();

        return services;
    }
}