using ESS.Application.Common.Interfaces;
using ESS.Infrastructure.Caching;
using ESS.Infrastructure.MultiTenancy.TenantResolution;
using ESS.Infrastructure.Persistence;
using ESS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ESS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Register IApplicationDbContext
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // Redis Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["Redis:Configuration"];
            options.InstanceName = "ESS_";
        });

        // Register Services
        services.AddScoped<IDbInitializer, DbInitializer>();
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ITenantResolver, CachingTenantResolver>();
        services.AddScoped<ITenantDatabaseService, TenantDatabaseService>();

        return services;
    }
}