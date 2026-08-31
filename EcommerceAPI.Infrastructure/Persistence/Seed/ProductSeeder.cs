using Bogus;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Infrastructure.Persistence.Seed
{
    public static class ProductSeeder
    {
        public static async Task SeedProductsAsync(AppDbContext context, int count = 500)
        {
            if (await context.Products.CountAsync() > 10) // don't reseed if real data already exists
            {
                return;
            }

            var categories = await context.Categories.ToListAsync();
            var tags = await context.Tags.ToListAsync();

            if (categories.Count == 0)
            {
                throw new InvalidOperationException("Seed categories before seeding products.");
            }

            var faker = new Faker<Product>()
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
                .RuleFor(p => p.Price, f => f.Random.Decimal(5, 2000))
                .RuleFor(p => p.StockQuantity, f => f.Random.Int(0, 500))
                .RuleFor(p => p.CreationDate, f => f.Date.Past(2))
                .RuleFor(p => p.CategoryId, f => f.PickRandom(categories).Id)
                .RuleFor(p => p.AltText, (f, p) => $"{p.Name} product image");

            var products = faker.Generate(count);

            // Ensure unique slugs (Commerce.ProductName() can repeat)
            var usedSlugs = new HashSet<string>();
            var random = Random.Shared;
            foreach (var product in products)
            {
                var baseSlug = product.Name
                    .ToLowerInvariant()
                    .Replace(" ", "-")
                    .Replace("'", "");

                var slug = baseSlug;
                var suffix = 1;
                while (!usedSlugs.Add(slug))
                {
                    slug = $"{baseSlug}-{suffix++}";
                }

                product.Slug = slug;

                // Randomly attach 0-3 tags per product, if tags exist
                if (tags.Count > 0)
                {
                    var tagCount = random.Next(0, Math.Min(4, tags.Count));
                    var pickedTags = tags.OrderBy(_ => Guid.NewGuid()).Take(tagCount);
                    product.ProductTags = pickedTags.Select(t => new ProductTag { TagId = t.Id }).ToList();
                }
            }

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}