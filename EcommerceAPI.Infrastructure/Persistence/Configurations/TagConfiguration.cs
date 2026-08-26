using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceAPI.Infrastructure.Persistence.Configurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Slug)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(t => t.Slug)
                .IsUnique();

            builder.HasIndex(t => t.Slug)
                .IsUnique();

            builder.HasData(
                new Tag { Id = 1, Name = "New Arrival" , Slug = "new-arrival" },
                new Tag { Id = 2, Name = "Best Seller", Slug = "best-seller" },
                new Tag { Id = 3, Name = "Sale" , Slug = "sale" }
                );
        }
    }
}