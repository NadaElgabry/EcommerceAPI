using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace EcommerceAPI.Infrastructure.Persistence.Configurations
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.UnitPrice)
                .HasColumnType("decimal(18,2)");

            builder.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(ci => new { ci.CartId, ci.ProductId })
                .IsUnique();

            builder.HasData(
                new CartItem
                {
                    Id = 1,
                    CartId = 1,
                    ProductId = 1,
                    Quantity = 2,
                    UnitPrice = 199.99m,
                    CreatedAt = new DateTime(2026, 8, 28, 13, 51, 1, 429, DateTimeKind.Utc).AddTicks(2431)
                },
                new CartItem
                {
                    Id = 2,
                    CartId = 1,
                    ProductId = 2,
                    Quantity = 3,
                    UnitPrice = 5.12m,
                    CreatedAt = new DateTime(2026, 8, 28, 13, 51, 1, 429, DateTimeKind.Utc).AddTicks(4965)
                }
            );
        }
    }
}