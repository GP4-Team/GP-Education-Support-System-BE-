using ESS.Application.Common.Interfaces;
using ESS.Domain.Entities;
using ESS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace ESS.Infrastructure.Services;

public class TenantMigrationTracker
{
    private readonly ILogger<TenantMigrationTracker> _logger;
    private readonly IApplicationDbContext _centralContext;

    public TenantMigrationTracker(
        ILogger<TenantMigrationTracker> logger,
        IApplicationDbContext centralContext)
    {
        _logger = logger;
        _centralContext = centralContext;
    }

    public async Task<IReadOnlyList<Tenant>> GetPendingMigrationTenantsAsync()
    {
        return await _centralContext.Tenants
            .Where(t => t.IsActive && t.DatabaseStatus != Domain.Enums.TenantDatabaseStatus.Active)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<string>> GetAppliedMigrationsAsync(string connectionString)
    {
        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            using var context = new TenantDbContext(optionsBuilder.Options, null!, null!);
            return (await context.Database.GetAppliedMigrationsAsync()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get applied migrations for connection string {ConnectionString}", connectionString);
            return new List<string>();
        }
    }

    public async Task<IReadOnlyList<string>> GetPendingMigrationsAsync(string connectionString)
    {
        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            using var context = new TenantDbContext(optionsBuilder.Options, null!, null!);
            return (await context.Database.GetPendingMigrationsAsync()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get pending migrations for connection string {ConnectionString}", connectionString);
            return new List<string>();
        }
    }
}