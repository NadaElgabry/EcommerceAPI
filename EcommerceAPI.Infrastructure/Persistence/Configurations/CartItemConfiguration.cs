using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

            builder.HasData(
                new CartItem
                {
                    Id = 1,
                    CartId = 1,
                    ProductId = 1,
                    quantity = 2,
                    UnitPrice = 199.99m
                },
                new CartItem
                {
                    Id = 2,
                    CartId = 1,
                    ProductId = 2,
                    quantity = 3,
                    UnitPrice = 5.12m
                }
            );
        }
    }
}