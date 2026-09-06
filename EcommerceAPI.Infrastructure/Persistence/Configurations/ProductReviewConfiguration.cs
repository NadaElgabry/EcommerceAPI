using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceAPI.Infrastructure.Persistence.Configurations
{
    public class ProductReviewConfiguration
        : IEntityTypeConfiguration<ProductReview>
    {
        public void Configure(EntityTypeBuilder<ProductReview> builder)
        {
            builder.HasKey(review => review.Id);

            builder.Property(review => review.Rating)
                .IsRequired();

            builder.Property(review => review.Comment)
                .IsRequired(false);

            builder.HasIndex(review => new
            {
                review.UserId,
                review.ProductId
            })
            .IsUnique();

            builder.HasOne(review => review.User)
                .WithMany()
                .HasForeignKey(review => review.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(review => review.Product)
                .WithMany()
                .HasForeignKey(review => review.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable(
                "ProductReviews",
                table => table.HasCheckConstraint(
                    "CK_ProductReviews_Rating",
                    "[Rating] >= 1 AND [Rating] <= 5"));
        }
    }
}
