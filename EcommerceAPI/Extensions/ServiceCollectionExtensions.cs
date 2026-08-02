using EcommerceAPI.Middlewares;

namespace EcommerceAPI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddSwaggerGen();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddCors(options =>
        {
            options.AddPolicy("Dev", policy =>
                policy.WithOrigins("http://localhost:5223", "https://localhost:7xxx")
                      .AllowAnyHeader()
                      .AllowAnyMethod());
        });

        return services;
    }
}