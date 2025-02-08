using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;

namespace ESS.Infrastructure.MultiTenancy;

public class EssTenantInfo : ITenantInfo
{
    string? ITenantInfo.Id { get; set; }
    string? ITenantInfo.Identifier { get; set; }
    public string? Name { get; set; }
    public string? ConnectionString { get; set; }

    // Public properties that hide the interface implementation
    public string Id
    {
        get => ((ITenantInfo)this).Id ?? string.Empty;
        set => ((ITenantInfo)this).Id = value;
    }

    public string Identifier
    {
        get => ((ITenantInfo)this).Identifier ?? string.Empty;
        set => ((ITenantInfo)this).Identifier = value;
    }
}