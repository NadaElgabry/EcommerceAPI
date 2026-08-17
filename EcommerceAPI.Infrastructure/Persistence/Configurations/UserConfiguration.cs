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

            builder.HasData(
                new User
                {
                    Id = 1,
                    Guid = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479"),
                    FirstName = "admin",
                    LastName = "user",
                    Role = Domain.Enums.Role.Admin,
                    Email = "admin@example.com",
                    // Initial password is : Password@123
                    HashedPassword = "$2y$10$7rLSvRVyTQORapkDOqmkhetjF6H9lJHngr4hJMSM2lHObJbW5EQh6",
                    IsActive=true,
                    BirthDate= new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    PhoneNumber ="01200032134"
                }
                );
        }
    }
}