namespace ESS.Domain.Entities;
public class TenantDomain
{
    public required Guid Id { get; set; }
    public required Guid TenantId { get; set; }
    public required string Domain { get; set; }
    public required bool IsPrimary { get; set; }
    public required bool IsActive { get; set; }
    public required DateTime CreatedAt { get; set; }
    public virtual Tenant? Tenant { get; set; }
}