//// src/Infrastructure/ESS.Infrastructure/Services/TenantService.cs
//using ESS.Application.Common.Interfaces;
//using ESS.Domain.Constants;
//using ESS.Domain.Entities;
//using ESS.Infrastructure.Persistence;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Npgsql;

//namespace ESS.Infrastructure.Services;

//public class TenantService : ITenantService
//{
//    private readonly ApplicationDbContext _context;
//    private readonly ICacheService _cacheService;
//    private readonly ILogger<TenantService> _logger;
//    private readonly IConfiguration _configuration;

//    public TenantService(
//        ApplicationDbContext context,
//        ICacheService cacheService,
//        ILogger<TenantService> logger,
//        IConfiguration configuration)
//    {
//        _context = context;
//        _cacheService = cacheService;
//        _logger = logger;
//        _configuration = configuration;
//    }
//    public async Task<Tenant?> GetTenantByIdAsync(Guid id)
//    {
//        var cacheKey = $"tenant_id_{id}";

//        // Try cache first
//        var tenant = await _cacheService.GetAsync<Tenant>(cacheKey);
//        if (tenant != null)
//        {
//            return tenant;
//        }

//        // Get from database
//        tenant = await _context.Tenants
//            .FirstOrDefaultAsync(t => t.Id == id);

//        if (tenant != null)
//        {
//            await _cacheService.SetAsync(cacheKey, tenant, TimeSpan.FromMinutes(30));
//        }

//        return tenant;
//    }

//    public async Task<Tenant?> GetTenantByIdentifierAsync(string identifier)
//    {
//        var cacheKey = $"tenant_identifier_{identifier}";

//        // Try cache first
//        var tenant = await _cacheService.GetAsync<Tenant>(cacheKey);
//        if (tenant != null)
//        {
//            return tenant;
//        }

//        // Get from database
//        tenant = await _context.Tenants
//            .FirstOrDefaultAsync(t => t.Identifier == identifier);

//        if (tenant != null)
//        {
//            await _cacheService.SetAsync(cacheKey, tenant, TimeSpan.FromMinutes(30));
//        }

//        return tenant;
//    }
//    public async Task<Tenant?> GetTenantByDomainAsync(string domain)
//    {
//        return await _context.TenantDomains
//            .Include(td => td.Tenant)
//            .Where(td => td.Domain == domain && td.Tenant != null && td.Tenant.IsActive)
//            .Select(td => td.Tenant)
//            .FirstOrDefaultAsync();
//    }

//    public async Task<IEnumerable<string>> GetAllTenantDomainsAsync()
//    {
//        return await _context.TenantDomains
//            .Where(td => td.Tenant != null && td.Tenant.IsActive)
//            .Select(td => td.Domain)
//            .ToListAsync();
//    }

//    public async Task InvalidateTenantCacheAsync(string domain)
//    {
//        var cacheKey = CacheKeys.TenantByDomain(domain);
//        await _cacheService.RemoveAsync(cacheKey);
//        _logger.LogInformation("Cache invalidated for domain {Domain}", domain);
//    }

//    public async Task<string> GetTenantConnectionStringAsync(string tenantId)
//    {
//        var cacheKey = CacheKeys.TenantConnectionString(tenantId);

//        // Try cache first
//        var connectionString = await _cacheService.GetAsync<string>(cacheKey);
//        if (!string.IsNullOrEmpty(connectionString))
//        {
//            return connectionString;
//        }

//        // Get tenant connection string
//        var tenant = await _context.Tenants
//            .FirstOrDefaultAsync(t => t.Id == Guid.Parse(tenantId) && t.IsActive);

//        if (tenant == null)
//        {
//            throw new InvalidOperationException($"Tenant {tenantId} not found or inactive");
//        }

//        // Cache the connection string
//        await _cacheService.SetAsync(cacheKey, tenant.ConnectionString, TimeSpan.FromHours(1));

//        return tenant.ConnectionString;
//    }
//}




// src/Infrastructure/ESS.Infrastructure/Services/TenantService.cs
using ESS.Application.Common.Interfaces;
using ESS.Domain.Constants;
using ESS.Domain.Entities;
using ESS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ESS.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<TenantService> _logger;
    private readonly IConfiguration _configuration;
    private string? _currentTenantId;

    public TenantService(
        ApplicationDbContext context,
        ICacheService cacheService,
        ILogger<TenantService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
        _configuration = configuration;
    }

    // Implement ITenantService interface methods
    public string GetTenantId()
    {
        if (string.IsNullOrEmpty(_currentTenantId))
        {
            throw new InvalidOperationException("Tenant context not found");
        }
        return _currentTenantId;
    }

    public bool IsTenantActive()
    {
        var tenantId = GetTenantId();
        var tenant = GetTenantByIdAsync(Guid.Parse(tenantId)).Result;
        return tenant?.IsActive ?? false;
    }

    // Existing methods
    public async Task<Tenant?> GetTenantByIdAsync(Guid id)
    {
        var cacheKey = $"tenant_id_{id}";
        // Try cache first
        var tenant = await _cacheService.GetAsync<Tenant>(cacheKey);
        if (tenant != null)
        {
            return tenant;
        }

        // Get from database
        tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == id);
        if (tenant != null)
        {
            await _cacheService.SetAsync(cacheKey, tenant, TimeSpan.FromMinutes(30));
        }
        return tenant;
    }

    public async Task<Tenant?> GetTenantByIdentifierAsync(string identifier)
    {
        var cacheKey = $"tenant_identifier_{identifier}";
        // Try cache first
        var tenant = await _cacheService.GetAsync<Tenant>(cacheKey);
        if (tenant != null)
        {
            return tenant;
        }

        // Get from database
        tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Identifier == identifier);
        if (tenant != null)
        {
            await _cacheService.SetAsync(cacheKey, tenant, TimeSpan.FromMinutes(30));
        }
        return tenant;
    }

    public async Task<Tenant?> GetTenantByDomainAsync(string domain)
    {
        return await _context.TenantDomains
            .Include(td => td.Tenant)
            .Where(td => td.Domain == domain && td.Tenant != null && td.Tenant.IsActive)
            .Select(td => td.Tenant)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<string>> GetAllTenantDomainsAsync()
    {
        return await _context.TenantDomains
            .Where(td => td.Tenant != null && td.Tenant.IsActive)
            .Select(td => td.Domain)
            .ToListAsync();
    }

    public async Task InvalidateTenantCacheAsync(string domain)
    {
        var cacheKey = CacheKeys.TenantByDomain(domain);
        await _cacheService.RemoveAsync(cacheKey);
        _logger.LogInformation("Cache invalidated for domain {Domain}", domain);
    }

    public async Task<string> GetTenantConnectionStringAsync(string tenantId)
    {
        var cacheKey = CacheKeys.TenantConnectionString(tenantId);
        // Try cache first
        var connectionString = await _cacheService.GetAsync<string>(cacheKey);
        if (!string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }

        // Get tenant connection string
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == Guid.Parse(tenantId) && t.IsActive);
        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant {tenantId} not found or inactive");
        }

        // Cache the connection string
        await _cacheService.SetAsync(cacheKey, tenant.ConnectionString, TimeSpan.FromHours(1));
        return tenant.ConnectionString;
    }

    // Internal method to set current tenant (should be called by middleware)
    internal void SetCurrentTenant(string tenantId)
    {
        _currentTenantId = tenantId;
    }
}