namespace ESS.Domain.Entities;

public class Tenant
{
    public Tenant()
    {
        Domains = new HashSet<TenantDomain>();
    }

    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Identifier { get; set; }
    public required string ConnectionString { get; set; }
    public required bool IsActive { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? LastUpdatedAt { get; set; }

    public virtual ICollection<TenantDomain> Domains { get; set; }
}