using ESS.Domain.Entities;

namespace ESS.Application.Common.Interfaces;

public interface ITenantService
{
    Task<Tenant?> GetTenantByDomainAsync(string domain);
    Task<Tenant?> GetTenantByIdAsync(Guid id);
    Task<Tenant?> GetTenantByIdentifierAsync(string identifier);
    Task<IEnumerable<string>> GetAllTenantDomainsAsync();
    Task InvalidateTenantCacheAsync(string domain);
    Task<string> GetTenantConnectionStringAsync(string tenantId);
    string GetTenantId();
    bool IsTenantActive();
}