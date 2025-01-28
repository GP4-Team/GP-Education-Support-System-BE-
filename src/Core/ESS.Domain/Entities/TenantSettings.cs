namespace ESS.Domain.Entities;
public class TenantSettings
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public virtual Tenant Tenant { get; set; }
}