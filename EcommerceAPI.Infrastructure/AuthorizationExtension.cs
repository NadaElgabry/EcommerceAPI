using Microsoft.Extensions.DependencyInjection;

namespace EcommerceAPI.Infrastructure
{
    public static class AuthorizationExtension
    {
        public static IServiceCollection AddAppAuthorization(
            this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("UsersRead", policy =>
                    policy.RequireAssertion(ctx =>
                        ctx.User.IsInRole("Admin") ||
                        ctx.User.HasClaim(
                            "scope",
                            "users:read")));

                options.AddPolicy("ReviewsRead", policy =>
                {
                    policy.RequireClaim(
                        "token_type",
                        "service");

                    policy.RequireClaim(
                        "scope",
                        "reviews:read");
                });
            });

            return services;
        }
    }
}
