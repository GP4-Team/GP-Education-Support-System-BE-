// src/Core/ESS.Application/Features/Media/EventHandlers/MediaAssociatedEventHandler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using ESS.Application.Common.Interfaces;
using ESS.Domain.Events.Media;


public class MediaAssociatedEventHandler : INotificationHandler<MediaAssociatedEvent>
{
    private readonly IMediaStorageService _storageService;
    private readonly ILogger<MediaAssociatedEventHandler> _logger;

    public MediaAssociatedEventHandler(
        IMediaStorageService storageService,
        ILogger<MediaAssociatedEventHandler> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    public async Task Handle(
        MediaAssociatedEvent notification,
        CancellationToken cancellationToken)
    {
        var media = notification.Media;
        try
        {
            await _storageService.MoveFileAsync(
                media.FilePath,
                $"media/{media.TenantId}/{media.ResourceType}/{media.ResourceId}/{media.Collection}/{media.File.FileName}",
                cancellationToken);

            _logger.LogInformation(
                "Moved media file from temporary to permanent storage: {FilePath}",
                media.FilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error moving media file: {FilePath}",
                media.FilePath);
            throw;
        }
    }
}