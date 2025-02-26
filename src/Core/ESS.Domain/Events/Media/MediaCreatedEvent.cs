// src/Core/ESS.Domain/Events/Media/MediaCreatedEvent.cs
using ESS.Domain.Common;
using ESS.Domain.Entities.Media;

namespace ESS.Domain.Events.Media;

public class MediaCreatedEvent : DomainEvent
{
    public Entities.Media.Media Media { get; }

    public MediaCreatedEvent(Entities.Media.Media media)
    {
        Media = media;
    }
}