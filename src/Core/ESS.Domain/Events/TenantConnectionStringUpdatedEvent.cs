using ESS.Domain.Common;

namespace ESS.Domain.Events;

public class TenantConnectionStringUpdatedEvent : DomainEvent
{
    public Guid TenantId { get; }
    public string ConnectionString { get; }

    public TenantConnectionStringUpdatedEvent(Guid tenantId, string connectionString)
    {
        TenantId = tenantId;
        ConnectionString = connectionString;
    }
}