using MediatR;
using Microsoft.Extensions.Logging;
using ESS.Application.Common.Interfaces;
using ESS.Domain.Events;

namespace ESS.Application.Features.Tenants.EventHandlers;

public class TenantConnectionStringUpdatedEventHandler : INotificationHandler<TenantConnectionStringUpdatedEvent>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<TenantConnectionStringUpdatedEventHandler> _logger;

    public TenantConnectionStringUpdatedEventHandler(
        ICacheService cacheService,
        ILogger<TenantConnectionStringUpdatedEventHandler> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task Handle(TenantConnectionStringUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling TenantConnectionStringUpdatedEvent for tenant {TenantId}", notification.TenantId);
        await _cacheService.RemoveAsync($"tenant-conn-{notification.TenantId}");
    }
}