using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceAPI.Infrastructure.Persistence.Configurations
{
    public class VerificationTokenConfiguration
        : IEntityTypeConfiguration<VerificationToken>
    {
        public void Configure(EntityTypeBuilder<VerificationToken> builder)
        {
            builder.Property(vt => vt.TokenHash)
                .HasMaxLength(88);

            builder.HasIndex(vt => vt.TokenHash)
                .IsUnique();

            builder.Property(vt => vt.Purpose)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.HasOne(vt => vt.User)
                .WithMany(u => u.VerificationTokens)
                .HasForeignKey(vt => vt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}