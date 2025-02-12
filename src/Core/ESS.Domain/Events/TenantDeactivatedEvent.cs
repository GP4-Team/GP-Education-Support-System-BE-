using ESS.Domain.Common;

namespace ESS.Domain.Events;

public class TenantDeactivatedEvent : DomainEvent
{
    public Guid TenantId { get; }
    public string TenantName { get; }

    public TenantDeactivatedEvent(Guid tenantId, string tenantName)
    {
        TenantId = tenantId;
        TenantName = tenantName;
    }
}