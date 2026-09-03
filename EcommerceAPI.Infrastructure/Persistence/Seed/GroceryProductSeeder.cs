using EcommerceAPI.Application.Interfaces.Image;
using EcommerceAPI.Application.Interfaces.Slug;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Domain.Enums;
using EcommerceAPI.Infrastructure.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Infrastructure.Persistence.Seed
{
    public static class GroceryProductSeeder
    {
        public static async Task SeedAsync(
            AppDbContext context,
            IImageService imageService,
            ISlugGenerator slugGenerator,
            string catalogJsonPath,
            string imagesRootFolder,
            CancellationToken cancellationToken = default)
        {
            if (await context.Products.CountAsync(cancellationToken) > 10) // don't reseed if real data already exists
            {
                return;
            }

            var categories = await context.Categories.ToListAsync(cancellationToken);
            if (categories.Count == 0)
            {
                throw new InvalidOperationException("Seed categories before seeding products.");
            }
            var categoryIdByName = categories.ToDictionary(c => c.Name, c => c.Id, StringComparer.OrdinalIgnoreCase);

            var existingProductSlugs = await context.Products.Select(p => p.Slug).ToListAsync(cancellationToken);
            var usedSlugs = new HashSet<string>(existingProductSlugs, StringComparer.OrdinalIgnoreCase);

            var existingTags = await context.Tags.ToListAsync(cancellationToken);
            var tagsBySlug = existingTags.ToDictionary(t => t.Slug, t => t, StringComparer.OrdinalIgnoreCase);

            foreach (var leaf in GroceryCatalogWalker.WalkCatalog(catalogJsonPath))
            {
                foreach (var tagName in leaf.Path[1..^1])
                {
                    var tagSlug = slugGenerator.GenerateSlug(tagName);
                    if (!tagsBySlug.ContainsKey(tagSlug))
                    {
                        var tag = new Tag { Name = tagName, Slug = tagSlug };
                        tagsBySlug[tagSlug] = tag;
                        context.Tags.Add(tag);
                    }
                }
            }
            await context.SaveChangesAsync(cancellationToken);

            // phase 2: create products, referencing the now-persisted tags
            var random = Random.Shared;

            foreach (var leaf in GroceryCatalogWalker.WalkCatalog(catalogJsonPath))
            {
                if (!categoryIdByName.TryGetValue(leaf.Path[0], out var categoryId))
                {
                    throw new InvalidOperationException($"Category '{leaf.Path[0]}' was not seeded. Run CategorySeeder first.");
                }

                var baseSlug = slugGenerator.GenerateSlug(leaf.Name);
                var slug = baseSlug;
                var suffix = 1;
                while (!usedSlugs.Add(slug))
                {
                    slug = $"{baseSlug}-{suffix++}";
                }

                var product = new Product
                {
                    Name = leaf.Name,
                    Slug = slug,
                    Description = leaf.Description,
                    AltText = $"{leaf.Name} product image",
                    Price = Math.Round((decimal)(random.NextDouble() * 195 + 5), 2),
                    StockQuantity = random.Next(0, 500),
                    CreationDate = DateTime.UtcNow.AddDays(-random.Next(0, 730)),
                    CategoryId = categoryId
                };

                // subcategory path segments become tags, e.g. Fruit/Apple/Golden-Delicious -> tag "Apple"
                foreach (var tagName in leaf.Path[1..^1])
                {
                    var tagSlug = slugGenerator.GenerateSlug(tagName);
                    product.ProductTags.Add(new ProductTag { Tag = tagsBySlug[tagSlug] });
                }

                var localFileName = leaf.ImageRelativePath.Replace('/', '_');
                var localImagePath = Path.Combine(imagesRootFolder, localFileName);

                if (File.Exists(localImagePath))
                {
                    await using var stream = File.OpenRead(localImagePath);
                    var formFile = new FormFile(stream, 0, stream.Length, "file", localFileName)
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = "image/jpeg"
                    };
                    product.ProductImage = await imageService.SaveFileAsync(formFile, slug, ImageOwnerType.Product, cancellationToken);
                }

                context.Products.Add(product);
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
