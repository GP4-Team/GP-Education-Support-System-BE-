using ESS.Infrastructure.MultiTenancy.TenantResolution;

namespace ESS.API.Extensions;

public static class TenantResolutionExtensions
{
    public static IApplicationBuilder UseTenantResolution(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantResolutionMiddleware>();
    }
}