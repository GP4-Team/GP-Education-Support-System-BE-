using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ESS.Infrastructure.MultiTenancy;

public static class TenantConfiguration
{
    public static IServiceCollection AddMultiTenancy(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMultiTenant<EssTenantInfo>()
            .WithDelegateStrategy((context) =>
            {
                if (context is HttpContext httpContext)
                {
                    var host = httpContext.Request.Host.Value;
                    return Task.FromResult<string?>(host.Split(':')[0]);
                }
                return Task.FromResult<string?>(null);
            })
            .WithStore<TenantStore>(ServiceLifetime.Scoped);

        return services;
    }
}