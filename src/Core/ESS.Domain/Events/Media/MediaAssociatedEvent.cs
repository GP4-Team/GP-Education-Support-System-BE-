// src/Core/ESS.Domain/Events/Media/MediaAssociatedEvent.cs
using ESS.Domain.Common;
using ESS.Domain.Entities.Media;

namespace ESS.Domain.Events.Media;

public class MediaAssociatedEvent : DomainEvent
{
    public Entities.Media.Media Media { get; }

    public MediaAssociatedEvent(Entities.Media.Media media)
    {
        Media = media;
    }
}