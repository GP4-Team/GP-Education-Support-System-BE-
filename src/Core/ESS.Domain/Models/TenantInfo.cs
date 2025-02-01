namespace ESS.Domain.Models;

public class TenantInfo
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string ConnectionString { get; set; }
    public bool IsActive { get; set; }
}