using ESS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ESS.Infrastructure.MultiTenancy;

public static class TenantContextExtensions
{
    public static Tenant? GetCurrentTenant(this HttpContext context)
    {
        var tenantAccessor = context.RequestServices.GetRequiredService<ITenantContextAccessor>();
        return tenantAccessor.CurrentTenant;
    }

    public static Tenant? GetCurrentTenant(this IServiceProvider services)
    {
        var tenantAccessor = services.GetRequiredService<ITenantContextAccessor>();
        return tenantAccessor.CurrentTenant;
    }

    public static void SetCurrentTenant(this HttpContext context, Tenant tenant)
    {
        var tenantAccessor = context.RequestServices.GetRequiredService<ITenantContextAccessor>();
        tenantAccessor.SetCurrentTenant(tenant);
    }
}