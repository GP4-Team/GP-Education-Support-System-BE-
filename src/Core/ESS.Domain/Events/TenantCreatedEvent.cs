using ESS.Domain.Common;

namespace ESS.Domain.Events;

public class TenantCreatedEvent : DomainEvent
{
    public Guid TenantId { get; }
    public string TenantName { get; }
    public string Identifier { get; }
    public bool UseSharedDatabase { get; }

    public TenantCreatedEvent(Guid tenantId, string tenantName, string identifier, bool useSharedDatabase)
    {
        TenantId = tenantId;
        TenantName = tenantName;
        Identifier = identifier;
        UseSharedDatabase = useSharedDatabase;
    }
}