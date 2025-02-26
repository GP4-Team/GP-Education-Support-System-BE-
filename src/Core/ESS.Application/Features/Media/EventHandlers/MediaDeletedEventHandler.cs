// src/Core/ESS.Application/Features/Media/EventHandlers/MediaDeletedEventHandler.cs

using MediatR;
using Microsoft.Extensions.Logging;
using ESS.Application.Common.Interfaces;
using ESS.Domain.Events.Media;
namespace ESS.Application.Features.Media.EventHandlers;

public class MediaDeletedEventHandler : INotificationHandler<MediaDeletedEvent>
{
    private readonly IMediaStorageService _storageService;
    private readonly ILogger<MediaDeletedEventHandler> _logger;

    public MediaDeletedEventHandler(
        IMediaStorageService storageService,
        ILogger<MediaDeletedEventHandler> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    public async Task Handle(
        MediaDeletedEvent notification,
        CancellationToken cancellationToken)
    {
        try
        {
            await _storageService.DeleteFileAsync(
                notification.Media.FilePath,
                cancellationToken);

            _logger.LogInformation(
                "Deleted media file: {FilePath}",
                notification.Media.FilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error deleting media file: {FilePath}",
                notification.Media.FilePath);
            throw;
        }
    }
}

