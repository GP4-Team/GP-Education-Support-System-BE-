using ESS.Application.Common.Interfaces;
using ESS.Domain.Constants;
using ESS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ESS.Infrastructure.MultiTenancy.TenantResolution;

public class CachingTenantResolver : ITenantResolver
{
    private readonly ICacheService _cacheService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<CachingTenantResolver> _logger;

    public CachingTenantResolver(
        ICacheService cacheService,
        ITenantService tenantService,
        ILogger<CachingTenantResolver> logger)
    {
        _cacheService = cacheService;
        _tenantService = tenantService;
        _logger = logger;
    }

    public async Task<Tenant?> ResolveTenantAsync(string host)
    {
        var cacheKey = CacheKeys.TenantByDomain(host);

        // Try cache first
        var tenant = await _cacheService.GetAsync<Tenant>(cacheKey);
        if (tenant != null)
        {
            _logger.LogDebug("Tenant {TenantId} resolved from cache for {Host}",
                tenant.Id, host);
            return tenant;
        }

        // Cache miss - check database
        tenant = await _tenantService.GetTenantByDomainAsync(host);
        if (tenant != null)
        {
            await _cacheService.SetAsync(cacheKey, tenant, TimeSpan.FromMinutes(30));
            _logger.LogInformation("Tenant {TenantId} cached for {Host}",
                tenant.Id, host);
        }

        return tenant;
    }
}