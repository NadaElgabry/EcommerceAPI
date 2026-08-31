using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceAPI.Infrastructure.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Slug)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(p => p.Slug)
                .IsUnique();

            builder.Property(p => p.Price)
                .HasColumnType("decimal(18,2)");


            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(
                new Product
                {
                    Id = 1,
                    Name = "Wireless Headphones",
                    Slug = "wireless-headphones",
                    Description = "High quality noise-canceling headphones.",
                    AltText = "Wireless Headphones",
                    ProductImage = "https://example.com/images/wireless-headphones.jpg",
                    Price = 199.99m,
                    StockQuantity = 50,
                    CreationDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    CategoryId = 1
                },
                new Product
                {
                    Id = 2,
                    Name = "Moro Dark Chocolate",
                    Slug = "moro-dark-chocolate",
                    Description = "Has chocolate in it.",
                    AltText = "Moro Dark Chocolate",
                    ProductImage = "https://example.com/images/moro-dark-chocolate.jpg",
                    Price = 5.12m,
                    StockQuantity = 100,
                    CreationDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    CategoryId = 2
                }


            );
        }
    }
}