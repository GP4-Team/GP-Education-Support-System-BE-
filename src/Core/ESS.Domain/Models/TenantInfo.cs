namespace ESS.Domain.Models;

public class TenantInfo
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string ConnectionString { get; set; }
    public bool IsActive { get; set; }
}