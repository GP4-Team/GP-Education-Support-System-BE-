// src/Core/ESS.Domain/Entities/Media/Media.cs
using ESS.Domain.Common;
using ESS.Domain.Events.Media;
using ESS.Domain.Interfaces;
using ESS.Domain.ValueObjects.Media;

namespace ESS.Domain.Entities.Media;

public class Media : BaseEntity, ITenantEntity
{
    public string TenantId { get; set; } = default!;
    public Guid ResourceId { get; private set; }
    public string ResourceType { get; private set; } = default!;
    public string Collection { get; private set; } = default!;
    public MediaFile File { get; private set; } = default!;
    public string FilePath { get; private set; } = default!;
    public bool IsTemporary { get; private set; }
    public Guid? TempGuid { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = default!;

    // Navigation properties
    public MediaCollection MediaCollection { get; private set; } = default!;
    public Guid MediaCollectionId { get; private set; }

    private Media() { } // For EF Core

    public static Media CreateTemporary(
        string tenantId,
        MediaFile file,
        string createdBy,
        Guid tempGuid,
        MediaCollection collection)
    {
        var media = new Media
        {
            TenantId = tenantId,
            File = file,
            CreatedBy = createdBy,
            IsTemporary = true,
            TempGuid = tempGuid,
            MediaCollection = collection,
            MediaCollectionId = collection.Id,
            CreatedAt = DateTime.UtcNow,
            FilePath = $"temp/{tempGuid}/{file.FileName}"
        };

        media.AddDomainEvent(new MediaCreatedEvent(media));
        return media;
    }

    public void AssociateWithResource(Guid resourceId, string resourceType, string collection)
    {
        if (!IsTemporary)
            throw new InvalidOperationException("Can only associate temporary media with resources.");

        ResourceId = resourceId;
        ResourceType = resourceType;
        Collection = collection;
        IsTemporary = false;
        TempGuid = null;
        FilePath = $"media/{TenantId}/{resourceType}/{resourceId}/{collection}/{File.FileName}";

        AddDomainEvent(new MediaAssociatedEvent(this));
    }

    public void MarkForDeletion()
    {
        AddDomainEvent(new MediaDeletedEvent(this));
    }
}
