using ESS.Domain.Entities;

namespace ESS.Infrastructure.MultiTenancy.TenantResolution;

public interface ITenantResolver
{
    Task<Tenant?> ResolveTenantAsync(string host);
}