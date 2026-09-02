using Amazon.S3;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Email;
using EcommerceAPI.Application.Interfaces.ExternalServices.Rag;
using EcommerceAPI.Application.Interfaces.Image;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Interfaces.Search;
using EcommerceAPI.Application.Interfaces.Slug;
using EcommerceAPI.Infrastructure.Contexts;
using EcommerceAPI.Infrastructure.ExternalServices.Rag;
using EcommerceAPI.Infrastructure.Persistence;
using EcommerceAPI.Infrastructure.Persistence.Repositories;
using EcommerceAPI.Infrastructure.Services.Auth;
using EcommerceAPI.Infrastructure.Services.Email;
using EcommerceAPI.Infrastructure.Services.Mail;
using EcommerceAPI.Infrastructure.Services.Search;
using EcommerceAPI.Infrastructure.Services.Search.Indexing;
using EcommerceAPI.Infrastructure.Services.Slug;
using EcommerceAPI.Infrastructure.Settings;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EcommerceAPI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<ISlugGenerator, SlugifySlugGenerator>();

        services.Configure<EmailSettings>(
           configuration.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IVerificationEmailTemplateProvider, VerificationEmailTemplateProvider>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.Configure<ElasticsearchSettings>(configuration.GetSection("Elasticsearch"));

        var esSettings = configuration.GetSection("Elasticsearch").Get<ElasticsearchSettings>()
            ?? throw new InvalidOperationException("Elasticsearch settings are not configured.");

        var esClientSettings = new ElasticsearchClientSettings(new Uri(esSettings.Url))
            .MaximumRetries(3)
            .RequestTimeout(TimeSpan.FromMinutes(2))
            .DefaultIndex(esSettings.ProductsIndex);

        services.AddSingleton(new ElasticsearchClient(esClientSettings));

        services.AddSingleton(typeof(ISearchService<>), typeof(ElasticSearchService<>));

        services.AddScoped<IProductIndexingService, ProductIndexingService>();

        services.AddScoped<IProductSearchService, ElasticProductSearchService>();
        services.AddJwtAuthentication(configuration);

        services.Configure<AwsSettings>(configuration.GetSection("AWS"));
        services.AddDefaultAWSOptions(configuration.GetAWSOptions());
        services.AddAWSService<IAmazonS3>();
        services.AddHttpClient<IRagClient, RagClient>();

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        services.Configure<JwtSettings>(jwtSection);

        var jwtSettings = jwtSection.Get<JwtSettings>()
            ?? throw new InvalidOperationException("Jwt settings are not configured.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                };
            });

        return services;
    }
}