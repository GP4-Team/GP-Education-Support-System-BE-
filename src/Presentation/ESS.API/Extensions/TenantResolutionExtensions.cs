using Microsoft.AspNetCore.Builder;

namespace ESS.Infrastructure.MultiTenancy.TenantResolution;

public static class TenantResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantResolutionMiddleware>();
    }
}