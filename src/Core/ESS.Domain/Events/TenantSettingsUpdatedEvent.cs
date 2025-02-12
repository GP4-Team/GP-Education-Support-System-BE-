using ESS.Domain.Common;

namespace ESS.Domain.Events;

public class TenantSettingsUpdatedEvent : DomainEvent
{
    public Guid TenantId { get; }
    public string Key { get; }
    public string Value { get; }

    public TenantSettingsUpdatedEvent(Guid tenantId, string key, string value)
    {
        TenantId = tenantId;
        Key = key;
        Value = value;
    }
}