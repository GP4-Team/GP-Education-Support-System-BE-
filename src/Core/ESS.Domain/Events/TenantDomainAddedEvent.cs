using ESS.Domain.Common;

namespace ESS.Domain.Events;

public class TenantDomainAddedEvent : DomainEvent
{
    public Guid TenantId { get; }
    public string Domain { get; }
    public bool IsPrimary { get; }

    public TenantDomainAddedEvent(Guid tenantId, string domain, bool isPrimary)
    {
        TenantId = tenantId;
        Domain = domain;
        IsPrimary = isPrimary;
    }
}