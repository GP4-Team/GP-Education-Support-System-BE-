// src/Core/ESS.Domain/Events/Media/MediaCollectionCreatedEvent.cs
using ESS.Domain.Common;
using ESS.Domain.Entities.Media;

namespace ESS.Domain.Events.Media;

public class MediaCollectionCreatedEvent : DomainEvent
{
    public MediaCollection MediaCollection { get; }

    public MediaCollectionCreatedEvent(MediaCollection mediaCollection)
    {
        MediaCollection = mediaCollection;
    }
}