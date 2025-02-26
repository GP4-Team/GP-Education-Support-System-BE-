// src/Infrastructure/ESS.Infrastructure/Persistence/Configurations/Media/MediaConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESS.Infrastructure.Persistence.Configurations.Media;

public class MediaConfiguration : IEntityTypeConfiguration<Domain.Entities.Media.Media>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Media.Media> builder)
    {
        builder.ToTable("Media");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ResourceType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Collection)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.FilePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.OwnsOne(x => x.File, fileBuilder =>
        {
            fileBuilder.Property(f => f.FileName)
                .HasColumnName("FileName")
                .IsRequired()
                .HasMaxLength(255);

            fileBuilder.Property(f => f.FileType)
                .HasColumnName("FileType")
                .IsRequired()
                .HasMaxLength(50);

            fileBuilder.Property(f => f.MimeType)
                .HasColumnName("MimeType")
                .IsRequired()
                .HasMaxLength(100);

            fileBuilder.Property(f => f.Size)
                .HasColumnName("Size")
                .IsRequired();
        });

        builder.HasIndex(x => new { x.TenantId, x.ResourceId, x.ResourceType });
        builder.HasIndex(x => new { x.TenantId, x.TempGuid })
            .HasFilter("\"IsTemporary\" = true");
    }
}