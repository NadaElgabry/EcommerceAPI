using Microsoft.Extensions.DependencyInjection;

namespace EcommerceAPI.Infrastructure
{
    public static class AuthorizationExtension
    {
        public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("UsersRead", policy =>
                    policy.RequireAssertion(ctx =>
                        ctx.User.IsInRole("Admin") ||
                        ctx.User.HasClaim("scope", "users:read")));
            });

            return services;
        }
    }
}