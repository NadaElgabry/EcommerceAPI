using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceAPI.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(u => u.Email)
                .HasMaxLength(256);

            builder.Property(u => u.IsActive)
                .HasConversion(
                    v => v.ToString(),
                    v => bool.Parse(v))
                .HasColumnType("nvarchar(10)");

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.HasIndex(u => u.Guid)
                .IsUnique();

            builder.HasMany(u => u.Addresses)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}