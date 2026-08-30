using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceAPI.Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.IdempotencyKey)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(o => o.IdempotencyKey)
                .IsUnique();

            builder.HasIndex(o => o.OrderNumber)
                .IsUnique();

            builder.HasIndex(o => o.Guid)
                .IsUnique();

            builder.Property(o => o.Address)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.HasOne(o => o.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(o => o.Items)
                .WithOne(i => i.order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasData(
                new Order
                {
                    Id = 1,
                    UserId = 1,
                    Guid = Guid.Parse("a0a0a0a0-a0a0-a0a0-a0a0-a0a0a0a0a0a0"),
                    OrderNumber = "100001",
                    IdempotencyKey = "seed-order-0001",
                    Address = "12 Tahrir Street, Giza, Egypt",
                    TotalAmount = 205.11m,
                    Status = OrderStatus.Delivered,
                    CreationDate = new DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Utc),
                    DeliveryTime = new DateTime(2024, 1, 8, 0, 0, 0, DateTimeKind.Utc)
                },
                new Order
                {
                    Id = 2,
                    UserId = 1,
                    Guid = Guid.Parse("b0b0b0b0-b0b0-b0b0-b0b0-b0b0b0b0b0b0"),
                    OrderNumber = "100002",
                    IdempotencyKey = "seed-order-0002",
                    Address = "12 Tahrir Street, Giza, Egypt",
                    TotalAmount = 5.12m,
                    Status = OrderStatus.Placed,
                    CreationDate = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                    DeliveryTime = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}