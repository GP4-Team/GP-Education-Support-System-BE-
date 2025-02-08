using ESS.Application.Common.Interfaces;
using ESS.Domain.Models;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;

namespace ESS.Infrastructure.MultiTenancy;

public class TenantStore : IMultiTenantStore<EssTenantInfo>
{
    private readonly ICacheService _cacheService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<TenantStore> _logger;

    public TenantStore(
        ICacheService cacheService,
        ITenantService tenantService,
        ILogger<TenantStore> logger)
    {
        _cacheService = cacheService;
        _tenantService = tenantService;
        _logger = logger;
    }

    public async Task<bool> TryAddAsync(EssTenantInfo tenantInfo)
    {
        try
        {
            var cacheKey = $"tenant:{tenantInfo.Identifier}";
            await _cacheService.SetAsync(cacheKey, tenantInfo, TimeSpan.FromHours(1));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding tenant to store");
            return false;
        }
    }

    public async Task<EssTenantInfo?> TryGetAsync(string id)
    {
        try
        {
            var cacheKey = $"tenant:{id}";
            return await _cacheService.GetAsync<EssTenantInfo>(cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant from store");
            return null;
        }
    }

    public async Task<EssTenantInfo?> TryGetByIdentifierAsync(string identifier)
    {
        try
        {
            // Try cache first
            var cacheKey = $"tenant:{identifier}";
            var tenantInfo = await _cacheService.GetAsync<EssTenantInfo>(cacheKey);

            if (tenantInfo != null)
                return tenantInfo;

            // Get from database
            var tenant = await _tenantService.GetTenantByIdentifierAsync(identifier);

            if (tenant == null)
                return null;

            tenantInfo = new EssTenantInfo
            {
                Id = tenant.Id.ToString(),
                Identifier = tenant.Identifier,
                Name = tenant.Name,
                ConnectionString = tenant.ConnectionString
            };

            // Cache the tenant info
            await TryAddAsync(tenantInfo);

            return tenantInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant by identifier from store");
            return null;
        }
    }

    public async Task<IEnumerable<EssTenantInfo>> GetAllAsync()
    {
        try
        {
            var domains = await _tenantService.GetAllTenantDomainsAsync();
            var tenants = new List<EssTenantInfo>();

            foreach (var domain in domains)
            {
                var tenant = await TryGetByIdentifierAsync(domain);
                if (tenant != null)
                {
                    tenants.Add(tenant);
                }
            }

            return tenants;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all tenants from store");
            return Enumerable.Empty<EssTenantInfo>();
        }
    }

    public async Task<bool> TryRemoveAsync(string identifier)
    {
        try
        {
            var cacheKey = $"tenant:{identifier}";
            await _cacheService.RemoveAsync(cacheKey);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing tenant from store");
            return false;
        }
    }

    public async Task<bool> TryUpdateAsync(EssTenantInfo tenantInfo)
    {
        try
        {
            await TryRemoveAsync(tenantInfo.Identifier);
            return await TryAddAsync(tenantInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant in store");
            return false;
        }
    }
}