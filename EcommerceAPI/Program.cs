using EcommerceAPI.Application;
using EcommerceAPI.Application.Interfaces.Image;
using EcommerceAPI.Application.Interfaces.Search;
using EcommerceAPI.Application.Interfaces.Slug;
using EcommerceAPI.Extensions;
using EcommerceAPI.Infrastructure;
using EcommerceAPI.Infrastructure.Contexts;
using EcommerceAPI.Infrastructure.Persistence.Seed;
using EcommerceAPI.Infrastructure.Services.Search.Indexing;
using EcommerceAPI.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
        var grocerySeedSettings = scope.ServiceProvider.GetRequiredService<IOptions<GrocerySeedSettings>>().Value;
        if (!string.IsNullOrWhiteSpace(grocerySeedSettings.CatalogJsonPath) &&
            !string.IsNullOrWhiteSpace(grocerySeedSettings.ImagesRootFolder) &&
            File.Exists(grocerySeedSettings.CatalogJsonPath) &&
            Directory.Exists(grocerySeedSettings.ImagesRootFolder))
        {
            var imageService = scope.ServiceProvider.GetRequiredService<IImageService>();
            var slugGenerator = scope.ServiceProvider.GetRequiredService<ISlugGenerator>();

            await CategorySeeder.SeedAsync(context, imageService, slugGenerator,
                grocerySeedSettings.CatalogJsonPath, grocerySeedSettings.ImagesRootFolder);

            await GroceryProductSeeder.SeedAsync(context, imageService, slugGenerator,
                grocerySeedSettings.CatalogJsonPath, grocerySeedSettings.ImagesRootFolder);
        }

        var indexingService = scope.ServiceProvider.GetRequiredService<IProductIndexingService>();
        await indexingService.ReindexAllProductsAsync();
    }
}

app.UseAppPipelineAsync();

app.Run();

