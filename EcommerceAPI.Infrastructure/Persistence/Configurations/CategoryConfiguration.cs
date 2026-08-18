using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceAPI.Infrastructure.Persistence.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Slug)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(c => c.Slug)
                .IsUnique();

            builder.Property(c => c.ImageUrl)
                .IsRequired();

            builder.HasData(
                new Category
                {
                    Id = 1,
                    Name = "Electronics",
                    ImageUrl = "https://www.flaticon.com/free-icon/electronics_1555401",
                    Slug = "electronics",
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Category
                {
                    Id = 2,
                    Name = "Groceries",
                    ImageUrl = "https://www.flaticon.com/free-icon/electronics_1555401",
                    Slug = "groceries",
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}