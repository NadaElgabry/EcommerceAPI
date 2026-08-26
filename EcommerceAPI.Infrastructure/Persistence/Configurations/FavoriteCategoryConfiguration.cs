using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceAPI.Infrastructure.Persistence.Configurations
{
    public class FavoriteCategoryConfiguration
        : IEntityTypeConfiguration<Domain.Entities.FavoriteCategory>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.FavoriteCategory> builder)
        {

            builder.HasIndex(f => new { f.UserId, f.CategoryId })
                .IsUnique();

            builder.HasOne(f => f.User).
                WithMany(u => u.PreferredCategories).
                HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(f => f.Category)
                .WithMany()
                .HasForeignKey(f => f.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}