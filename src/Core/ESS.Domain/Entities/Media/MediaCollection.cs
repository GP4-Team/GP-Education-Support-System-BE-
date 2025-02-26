// src/Core/ESS.Domain/Entities/Media/MediaCollection.cs
using ESS.Domain.Common;
using ESS.Domain.Events.Media;
using ESS.Domain.Interfaces;

namespace ESS.Domain.Entities.Media;

public class MediaCollection : BaseEntity, ITenantEntity
{
    public string TenantId { get; set; } = default!;
    public string Name { get; private set; } = default!;
    public string AllowedTypes { get; private set; } = default!;
    public long MaxFileSize { get; private set; }

    // Navigation property
    public ICollection<Media> Media { get; private set; } = new List<Media>();

    private MediaCollection() { } // For EF Core

    public static MediaCollection Create(
        string tenantId,
        string name,
        string allowedTypes,
        long maxFileSize)
    {
        var collection = new MediaCollection
        {
            TenantId = tenantId,
            Name = name,
            AllowedTypes = allowedTypes,
            MaxFileSize = maxFileSize
        };

        collection.AddDomainEvent(new MediaCollectionCreatedEvent(collection));
        return collection;
    }

    public bool IsFileTypeAllowed(string fileType)
    {
        var allowedTypesList = AllowedTypes.Split(',')
            .Select(t => t.Trim().ToLower());
        return allowedTypesList.Contains(fileType.ToLower());
    }

    public bool IsFileSizeAllowed(long size)
    {
        return size <= MaxFileSize;
    }
}