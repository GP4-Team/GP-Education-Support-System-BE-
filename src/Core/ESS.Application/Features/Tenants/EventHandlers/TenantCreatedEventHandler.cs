using ESS.Application.Common.Interfaces;
using ESS.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

public class TenantCreatedEventHandler : INotificationHandler<TenantCreatedEvent>
{
    private readonly ICacheService _cacheService;
    private readonly ITenantDatabaseService _tenantDbService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<TenantCreatedEventHandler> _logger;

    public TenantCreatedEventHandler(
        ICacheService cacheService,
        ITenantDatabaseService tenantDbService,
        IApplicationDbContext context,
        ILogger<TenantCreatedEventHandler> logger)
    {
        _cacheService = cacheService;
        _tenantDbService = tenantDbService;
        _context = context;
        _logger = logger;
    }

    public async Task Handle(TenantCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling TenantCreatedEvent for tenant {TenantName} ({TenantId})",
            notification.TenantName, notification.TenantId);

        if (!notification.UseSharedDatabase)
        {
            var tenant = await _context.Tenants.FindAsync(notification.TenantId);
            if (tenant != null)
            {
                await _tenantDbService.CreateTenantDatabaseAsync(tenant);
            }
            else
            {
                _logger.LogWarning("Tenant {TenantId} not found when trying to create database", notification.TenantId);
            }
        }

        await _cacheService.RemoveAsync($"tenant-{notification.Identifier}");
    }
}