using EcommerceAPI.Application.Interfaces.Image;
using EcommerceAPI.Application.Interfaces.Slug;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Domain.Enums;
using EcommerceAPI.Infrastructure.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Infrastructure.Persistence.Seed
{
    public static class CategorySeeder
    {
        public static async Task SeedAsync(
            AppDbContext context,
            IImageService imageService,
            ISlugGenerator slugGenerator,
            string catalogJsonPath,
            string imagesRootFolder,
            CancellationToken cancellationToken = default)
        {
            var existingSlugs = await context.Categories
                .Select(c => c.Slug)
                .ToListAsync(cancellationToken);
            var existing = new HashSet<string>(existingSlugs, StringComparer.OrdinalIgnoreCase);

            // one representative leaf per top-level key, used to source the category's image
            var firstLeafByTopLevel = new Dictionary<string, GroceryCatalogWalker.CatalogLeaf>(StringComparer.OrdinalIgnoreCase);
            foreach (var leaf in GroceryCatalogWalker.WalkCatalog(catalogJsonPath))
            {
                firstLeafByTopLevel.TryAdd(leaf.Path[0], leaf);
            }

            foreach (var (topLevelName, leaf) in firstLeafByTopLevel)
            {
                var slug = slugGenerator.GenerateSlug(topLevelName);
                if (existing.Contains(slug))
                {
                    continue;
                }

                var localFileName = leaf.ImageRelativePath.Replace('/', '_');
                var localImagePath = Path.Combine(imagesRootFolder, localFileName);

                if (!File.Exists(localImagePath))
                {
                    throw new FileNotFoundException($"Category seed image not found for '{topLevelName}': {localImagePath}");
                }

                string imageUrl;
                await using (var stream = File.OpenRead(localImagePath))
                {
                    var formFile = new FormFile(stream, 0, stream.Length, "file", localFileName)
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = "image/jpeg"
                    };
                    imageUrl = await imageService.SaveFileAsync(formFile, slug, ImageOwnerType.Category, cancellationToken);
                }

                context.Categories.Add(new Category
                {
                    Name = topLevelName,
                    Slug = slug,
                    ImageUrl = imageUrl,
                    CreatedAt = DateTime.UtcNow
                });

                existing.Add(slug);
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
