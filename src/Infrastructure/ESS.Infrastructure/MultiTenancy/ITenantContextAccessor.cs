using ESS.Domain.Entities;

namespace ESS.Infrastructure.MultiTenancy;

public interface ITenantContextAccessor
{
    Tenant? CurrentTenant { get; }
    void SetCurrentTenant(Tenant tenant);
    void ClearCurrentTenant();
}