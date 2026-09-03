using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceAPI.Infrastructure.Persistence.Configurations
{
    public class ServiceClientConfiguration : IEntityTypeConfiguration<ServiceClient>
    {
        public void Configure(EntityTypeBuilder<ServiceClient> builder)
        {
            builder.HasKey(c => c.Id);
            builder.HasIndex(c => c.ClientId).IsUnique();
            builder.Property(c => c.ClientId).HasMaxLength(64).IsRequired();
            builder.Property(c => c.ClientSecretHash).IsRequired();
            builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
            builder.Property(c => c.ScopesCsv).HasMaxLength(500).IsRequired();
        }
    }
}