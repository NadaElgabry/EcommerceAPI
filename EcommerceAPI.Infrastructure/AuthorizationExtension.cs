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

                options.AddPolicy("ProductsRead", policy =>
                {
                    policy.RequireClaim(
                        "token_type",
                        "service");

                    policy.RequireClaim(
                        "scope",
                        "products:read");
                });

                options.AddPolicy("ActivitiesRead", policy =>
                {
                    policy.RequireClaim(
                        "token_type",
                        "service");

                    policy.RequireClaim(
                        "scope",
                        "activities:read");
                });
            });

            return services;
        }
    }
}