using ESS.Domain.Entities;

namespace ESS.Application.Common.Interfaces;

public interface ITenantService
{
    Task<Tenant?> GetTenantByDomainAsync(string domain);
    Task<IEnumerable<string>> GetAllTenantDomainsAsync();
    Task InvalidateTenantCacheAsync(string domain);
    Task<string> GetTenantConnectionStringAsync(string tenantId);
}