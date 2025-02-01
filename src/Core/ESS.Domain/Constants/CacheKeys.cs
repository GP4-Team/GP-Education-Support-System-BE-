namespace ESS.Domain.Constants;

public static class CacheKeys
{
    public static string TenantByDomain(string domain) => $"tenant_domain_{domain}";
    public static string TenantConnectionString(string tenantId) => $"tenant_connection_{tenantId}";
}