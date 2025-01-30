using ESS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESS.Infrastructure.Persistence.Configurations;

public class TenantDomainConfiguration : IEntityTypeConfiguration<TenantDomain>
{
    public void Configure(EntityTypeBuilder<TenantDomain> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Domain)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(e => e.Domain)
            .IsUnique()
            .HasDatabaseName("IX_TenantDomains_Domain");

        builder.HasIndex(e => new { e.TenantId, e.IsPrimary })
            .HasFilter("\"IsPrimary\" = true")
            .IsUnique()
            .HasDatabaseName("IX_TenantDomains_TenantId_IsPrimary");
    }
}