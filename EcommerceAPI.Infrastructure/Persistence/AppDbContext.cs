using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Infrastructure.Contexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }


        public DbSet<UserAddress> UserAddresses { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<VerificationToken> VerificationTokens { get; set; }

        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .HasMany(user => user.Addresses)
                .WithOne(address => address.User)
                .HasForeignKey(address => address.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(user => user.RefreshTokens)
                .WithOne(refreshToken => refreshToken.User)
                .HasForeignKey(refreshToken => refreshToken.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>()
                .HasMany(user => user.VerificationTokens)
                .WithOne(verificationToken => verificationToken.User)
                .HasForeignKey(verificationToken => verificationToken.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .Property(user => user.Email)
                .HasMaxLength(256);

            modelBuilder.Entity<RefreshToken>()
                .Property(refreshToken => refreshToken.TokenHash)
                .HasMaxLength(88);

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(e => e.isActive)
                .HasConversion(
                    v => v.ToString(),    
                    v => bool.Parse(v)           
                )
                .HasColumnType("nvarchar(10)");
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(refreshToken => refreshToken.TokenHash)
                .IsUnique();

            modelBuilder.Entity<VerificationToken>()
                .Property(verificationToken => verificationToken.TokenHash)
                .HasMaxLength(88);

            modelBuilder.Entity<VerificationToken>()
                .HasIndex(verificationToken => verificationToken.TokenHash)
                .IsUnique();

            modelBuilder.Entity<VerificationToken>()
                .Property(vt => vt.Purpose)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Category>()
                .Property(c => c.Name)
                .HasMaxLength(100);

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

        }
    }
}