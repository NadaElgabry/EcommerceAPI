using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceAPI.Infrastructure.Persistence.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.UnitPrice)
                .HasColumnType("decimal(18,2)");

            builder.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasData(
                new OrderItem
                {
                    Id = 1,
                    OrderId = 1,
                    ProductId = 1,
                    Quantity = 1,
                    UnitPrice = 199.99m,
                    ProductName = "Wireless Headphones",
                    ProductSlug = "wireless-headphones",
                    ProductDescription = "High quality noise-canceling headphones.",
                    ProductImageUrl = "https://example.com/images/wireless-headphones.jpg",
                    ProductAltText = "Wireless Headphones"
                },
                new OrderItem
                {
                    Id = 2,
                    OrderId = 1,
                    ProductId = 2,
                    Quantity = 1,
                    UnitPrice = 5.12m,
                    ProductName = "Moro Dark Chocolate",
                    ProductSlug = "moro-dark-chocolate",
                    ProductDescription = "Has chocolate in it.",
                    ProductImageUrl = "https://example.com/images/moro-dark-chocolate.jpg",
                    ProductAltText = "Moro Dark Chocolate"
                },
                new OrderItem
                {
                    Id = 3,
                    OrderId = 2,
                    ProductId = 2,
                    Quantity = 1,
                    UnitPrice = 5.12m,
                    ProductName = "Moro Dark Chocolate",
                    ProductSlug = "moro-dark-chocolate",
                    ProductDescription = "Has chocolate in it.",
                    ProductImageUrl = "https://example.com/images/moro-dark-chocolate.jpg",
                    ProductAltText = "Moro Dark Chocolate"
                }
            );
        }
    }
}