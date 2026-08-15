using EcommerceAPI.Application;
using EcommerceAPI.Extensions;
using EcommerceAPI.Infrastructure;

using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Domain.Enums;
using EcommerceAPI.Infrastructure.Contexts;
using EcommerceAPI.Application.Interfaces.Auth;



var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPresentation();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    bool adminExists = context.Users.Any(u => u.Role == Role.Admin);

    if (!adminExists)
    {
        var admin = new User
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@ecommerce.com",
            PhoneNumber = "0000000000",
            HashedPassword = passwordHasher.Hash("Password@123"), // see note below
            Role = Role.Admin,
            isActive = true // skip email verification for the seeded admin
        };

        context.Users.Add(admin);
        context.SaveChanges();

        Console.WriteLine("=================================");
        Console.WriteLine($"Seeded admin: {admin.Email} / ChangeMe123!");
        Console.WriteLine("CHANGE THIS PASSWORD IMMEDIATELY");
        Console.WriteLine("=================================");
    }
}

app.UseAppPipeline();

app.Run();