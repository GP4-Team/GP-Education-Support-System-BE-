using MediatR;
using Microsoft.Extensions.Logging;
using ESS.Application.Common.Interfaces;
using ESS.Domain.Events;

namespace ESS.Application.Features.Tenants.EventHandlers;

public class TenantDeactivatedEventHandler : INotificationHandler<TenantDeactivatedEvent>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<TenantDeactivatedEventHandler> _logger;

    public TenantDeactivatedEventHandler(
        ICacheService cacheService,
        ILogger<TenantDeactivatedEventHandler> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task Handle(TenantDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling TenantDeactivatedEvent for tenant {TenantName} ({TenantId})",
            notification.TenantName, notification.TenantId);

        await _cacheService.RemoveAsync($"tenant-{notification.TenantId}");
        await _cacheService.RemoveAsync($"tenant-conn-{notification.TenantId}");
    }
}