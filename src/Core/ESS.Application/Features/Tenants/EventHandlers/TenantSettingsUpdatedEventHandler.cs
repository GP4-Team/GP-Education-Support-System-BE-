using MediatR;
using Microsoft.Extensions.Logging;
using ESS.Application.Common.Interfaces;
using ESS.Domain.Events;

namespace ESS.Application.Features.Tenants.EventHandlers;

public class TenantSettingsUpdatedEventHandler : INotificationHandler<TenantSettingsUpdatedEvent>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<TenantSettingsUpdatedEventHandler> _logger;

    public TenantSettingsUpdatedEventHandler(
        ICacheService cacheService,
        ILogger<TenantSettingsUpdatedEventHandler> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task Handle(TenantSettingsUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling TenantSettingsUpdatedEvent for tenant {TenantId}, key: {Key}",
            notification.TenantId, notification.Key);

        await _cacheService.RemoveAsync($"tenant-settings-{notification.TenantId}");
    }
}