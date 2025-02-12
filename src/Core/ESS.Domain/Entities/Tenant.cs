using ESS.Domain.Common;
using ESS.Domain.Events;

namespace ESS.Domain.Entities;

public class Tenant : BaseEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Identifier { get; private set; } = string.Empty;
    public string? ConnectionString { get; private set; }
    public bool IsActive { get; private set; }
    public bool UseSharedDatabase { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastUpdatedAt { get; private set; }
    public ICollection<TenantDomain> Domains { get; private set; } = new List<TenantDomain>();
    public ICollection<TenantSettings> Settings { get; private set; } = new List<TenantSettings>();

    private Tenant() { } // For EF Core

    public static Tenant Create(string name, string identifier, bool useSharedDatabase)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name,
            Identifier = identifier,
            UseSharedDatabase = useSharedDatabase,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        tenant.AddDomainEvent(new TenantCreatedEvent(tenant.Id, tenant.Name, tenant.Identifier, tenant.UseSharedDatabase));
        return tenant;
    }

    public void UpdateConnectionString(string connectionString)
    {
        ConnectionString = connectionString;
        LastUpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TenantConnectionStringUpdatedEvent(Id, connectionString));
    }

    public void AddDomain(string domain, bool isPrimary = false)
    {
        var tenantDomain = new TenantDomain
        {
            Id = Guid.NewGuid(),
            TenantId = Id,
            Domain = domain,
            IsPrimary = isPrimary,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        Domains.Add(tenantDomain);
        AddDomainEvent(new TenantDomainAddedEvent(Id, domain, isPrimary));
    }

    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        LastUpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TenantDeactivatedEvent(Id, Name));
    }

    public void UpdateSettings(string key, string value)
    {
        var setting = Settings.FirstOrDefault(s => s.Key == key);
        if (setting == null)
        {
            setting = new TenantSettings
            {
                Id = Guid.NewGuid(),
                TenantId = Id,
                Key = key,
                Value = value
            };
            Settings.Add(setting);
        }
        else
        {
            setting.Value = value;
        }

        LastUpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TenantSettingsUpdatedEvent(Id, key, value));
    }
}