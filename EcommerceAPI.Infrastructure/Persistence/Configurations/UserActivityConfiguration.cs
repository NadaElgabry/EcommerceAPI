using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceAPI.Infrastructure.Persistence.Configurations
{
    public class UserActivityConfiguration : IEntityTypeConfiguration<UserActivity>
    {
        public void Configure(EntityTypeBuilder<UserActivity> builder)
        {
            builder.HasKey(ua => ua.Id);

            builder.Property(ua => ua.ActionType)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.HasOne(ua => ua.User)
                .WithMany()
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(ua => ua.UserGuid);

            builder.HasOne(ua => ua.Product)
                .WithMany()
                .HasForeignKey(ua => ua.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}