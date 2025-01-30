namespace ESS.Domain.Entities;
public class TenantSettings
{
    public required Guid Id { get; set; }
    public required Guid TenantId { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }
    public virtual Tenant? Tenant { get; set; }
}