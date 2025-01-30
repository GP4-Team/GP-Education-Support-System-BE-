// ESS.Infrastructure/DependencyInjection.cs
using ESS.Application.Common.Interfaces;
using ESS.Infrastructure.Caching;
using ESS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace ESS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var infrastructureConfig = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("infrastructure.settings.json", optional: true)
        .Build();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["Redis:Configuration"];
            options.InstanceName = "ESS_";
        });

        services.AddScoped<IDbInitializer, DbInitializer>();
        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }
}