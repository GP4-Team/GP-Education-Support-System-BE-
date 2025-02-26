// src/Core/ESS.Domain/Events/Media/MediaDeletedEvent.cs
using ESS.Domain.Common;
using ESS.Domain.Entities.Media;

namespace ESS.Domain.Events.Media;

public class MediaDeletedEvent : DomainEvent
{
    public Entities.Media.Media Media { get; }

    public MediaDeletedEvent(Entities.Media.Media media)
    {
        Media = media;
    }
}