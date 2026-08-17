using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceAPI.Infrastructure.Persistence.Configurations
{
    public class UserCategoryConfiguration
        : IEntityTypeConfiguration<UserCategory>
    {
        public void Configure(EntityTypeBuilder<UserCategory> builder)
        {

            builder.HasKey(uc => new 
            { 
                uc.UserId, uc.CategoryId 
            });

            builder.HasOne(uc => uc.User)
                .WithMany(u => u.PreferredCategories)
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(uc => uc.Category)
                .WithMany()
                .HasForeignKey(uc => uc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}