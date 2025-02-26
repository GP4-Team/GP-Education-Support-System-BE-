// src/Infrastructure/ESS.Infrastructure/Persistence/Configurations/Media/MediaCollectionConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ESS.Domain.Entities.Media;

namespace ESS.Infrastructure.Persistence.Configurations.Media;

public class MediaCollectionConfiguration : IEntityTypeConfiguration<MediaCollection>
{
    public void Configure(EntityTypeBuilder<MediaCollection> builder)
    {
        builder.ToTable("MediaCollections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.AllowedTypes)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.MaxFileSize)
            .IsRequired();

        // Create index
        builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
    }
}