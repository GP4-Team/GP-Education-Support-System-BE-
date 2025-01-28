namespace ESS.Domain.Entities;
public class TenantAuditLog
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Action { get; set; }
    public string Details { get; set; }
    public DateTime Timestamp { get; set; }
    public virtual Tenant Tenant { get; set; }
}