using ESS.Domain.Enums;

namespace ESS.Domain.Entities;

public class Tenant
{
    public Tenant()
    {
        Domains = new HashSet<TenantDomain>();
        DatabaseStatus = TenantDatabaseStatus.Pending;
    }

    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Identifier { get; set; }
    public required string ConnectionString { get; set; }
    public required bool IsActive { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? LastUpdatedAt { get; set; }

    // Database tracking properties
    public TenantDatabaseStatus DatabaseStatus { get; set; }
    public DateTime? DatabaseCreatedAt { get; set; }
    public string? DatabaseError { get; set; }

    public virtual ICollection<TenantDomain> Domains { get; set; }
}