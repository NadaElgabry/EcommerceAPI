using EcommerceAPI.Filters;
using EcommerceAPI.Middlewares;
using IdempotentAPI.Cache.DistributedCache.Extensions.DependencyInjection;
using IdempotentAPI.Extensions.DependencyInjection;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.OpenApi;

namespace EcommerceAPI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddScoped<ValidationFilter>();

        services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>();
        });

        services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        services.AddSwagger();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddIdempotentAPI(new IdempotentAPI.Core.IdempotencyOptions
        {
            ExpireHours = 1
        });
        services.AddDistributedMemoryCache();

        services.AddIdempotentAPIUsingDistributedCache(); ;

        services.AddCors(options =>
        {
            options.AddPolicy("Dev", policy =>
                policy.WithOrigins("http://localhost:5223", "https://localhost:7xxx")
                      .AllowAnyHeader()
                      .AllowAnyMethod());
        });

        return services;
    }
    private static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token below (no 'Bearer ' prefix needed).\n\nExample: \"eyJhbGciOi...\""
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });
        services.AddFluentValidationRulesToSwagger();
        return services;
    }
}