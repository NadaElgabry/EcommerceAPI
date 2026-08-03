using System.Text;
using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Iservices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Services.User;
using EcommerceAPI.Application.UseCases.Auth;
using EcommerceAPI.Application.UseCases.Auth.Validators;
using EcommerceAPI.Infrastructure.Contexts;
using EcommerceAPI.Infrastructure.Presistence;
using EcommerceAPI.Infrastructure.Presistence.Repositories;
using EcommerceAPI.Infrastructure.Services.Auth;
using EcommerceAPI.Middlewares;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override(
        "Microsoft.AspNetCore",
        LogEventLevel.Warning
    )
    .Enrich.FromLogContext()
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        outputTemplate:
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} " +
            "[{Level:u3}] {Message:lj} {Properties:j}" +
            "{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddExceptionHandler<
    GlobalExceptionHandler
>();

builder.Services.AddProblemDetails();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection"
        )
    )
);

builder.Services.AddScoped<
    IUserRepository,
    UserRepository
>();

builder.Services.AddScoped<
    IRoleRepository,
    RoleRepository
>();

builder.Services.AddScoped<
    IRefreshTokenRepository,
    RefreshTokenRepository
>();

builder.Services.AddScoped<
    IUnitOfWork,
    UnitOfWork
>();

builder.Services.AddScoped<
    IPasswordHasher,
    PasswordHasher
>();

builder.Services.AddScoped<
    ITokenService,
    TokenService
>();

builder.Services.AddScoped<
    IUserService,
    UserService
>();

builder.Services.AddScoped<
    IValidator<RegisterRequest>,
    RegisterRequestValidator
>();

builder.Services.AddScoped<RegisterUseCase>();

var jwtSettings =
    builder.Configuration.GetSection("Jwt");

string key =
    jwtSettings["Key"]
    ?? throw new InvalidOperationException(
        "JWT key is missing from configuration."
    );

builder.Services.Configure<JwtSettings>(
    jwtSettings
);

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme
    )
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    jwtSettings["Issuer"],

                ValidAudience =
                    jwtSettings["Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(key)
                    )
            };
    });

var app = builder.Build();

app.UseExceptionHandler();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} " +
        "responded {StatusCode} " +
        "in {Elapsed:0.0000} ms";
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();