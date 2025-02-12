using MediatR;
using Microsoft.Extensions.Logging;
using ESS.Application.Common.Interfaces;
using ESS.Domain.Events;

namespace ESS.Application.Features.Tenants.EventHandlers;

public class TenantDomainAddedEventHandler : INotificationHandler<TenantDomainAddedEvent>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<TenantDomainAddedEventHandler> _logger;

    public TenantDomainAddedEventHandler(
        ICacheService cacheService,
        ILogger<TenantDomainAddedEventHandler> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task Handle(TenantDomainAddedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling TenantDomainAddedEvent for tenant {TenantId}, domain: {Domain}",
            notification.TenantId, notification.Domain);

        await _cacheService.RemoveAsync($"tenant-domain-{notification.Domain}");
    }
}