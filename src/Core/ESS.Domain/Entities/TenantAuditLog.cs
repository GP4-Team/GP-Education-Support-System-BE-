namespace ESS.Domain.Entities;
public class TenantAuditLog
{
    public required Guid Id { get; set; }
    public required Guid TenantId { get; set; }
    public required string Action { get; set; }
    public required string Details { get; set; }
    public required DateTime Timestamp { get; set; }
    public virtual Tenant? Tenant { get; set; }
}