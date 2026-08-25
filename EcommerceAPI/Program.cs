using EcommerceAPI.Application;
using EcommerceAPI.Application.Interfaces.Search;
using EcommerceAPI.Extensions;
using EcommerceAPI.Infrastructure;
using EcommerceAPI.Infrastructure.Contexts;
using EcommerceAPI.Infrastructure.Persistence.Seed;
using EcommerceAPI.Infrastructure.Services.Search.Indexing;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPresentation();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.EnsureProductsIndexExistsAsync();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
    if (app.Environment.IsDevelopment())
    {
        await ProductSeeder.SeedProductsAsync(context, count: 500);
        var indexingService = scope.ServiceProvider.GetRequiredService<IProductIndexingService>();
        await indexingService.ReindexAllProductsAsync();
    }
}

app.UseAppPipelineAsync();

app.Run();

