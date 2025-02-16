using ESS.Application.Common.Interfaces;
using ESS.Domain.Enums;
using ESS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ESS.Infrastructure.Services;

public class DatabaseMigrationService
{
    private readonly IApplicationDbContext _centralContext;
    private readonly ITenantService _tenantService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseMigrationService> _logger;

    public DatabaseMigrationService(
        IApplicationDbContext centralContext,
        ITenantService tenantService,
        IConfiguration configuration,
        ILogger<DatabaseMigrationService> logger)
    {
        _centralContext = centralContext;
        _tenantService = tenantService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> UpdateAllDatabasesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Update Central Database
            _logger.LogInformation("Starting central database migration");
            if (!await UpdateCentralDatabaseAsync(cancellationToken))
            {
                return false;
            }

            // 2. Get all active tenants
            var tenants = await _centralContext.Tenants
                .Where(t => t.IsActive)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} active tenants to update", tenants.Count);

            // 3. Update each tenant database
            var failedTenants = new List<(Guid Id, string Error)>();

            foreach (var tenant in tenants)
            {
                try
                {
                    _logger.LogInformation("Starting migration for tenant {TenantId}", tenant.Id);

                    tenant.SetDatabaseStatus(TenantDatabaseStatus.Migrating);
                    await _centralContext.SaveChangesAsync(cancellationToken);

                    if (await UpdateTenantDatabaseAsync(tenant.ConnectionString!, cancellationToken))
                    {
                        tenant.SetDatabaseStatus(TenantDatabaseStatus.Active);
                        _logger.LogInformation("Successfully migrated tenant {TenantId}", tenant.Id);
                    }
                    else
                    {
                        tenant.SetDatabaseStatus(TenantDatabaseStatus.MigrationFailed, "Migration failed");
                        failedTenants.Add((tenant.Id, "Migration failed"));
                        _logger.LogError("Failed to migrate tenant {TenantId}", tenant.Id);
                    }

                    await _centralContext.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    tenant.SetDatabaseStatus(TenantDatabaseStatus.MigrationFailed, ex.Message);
                    await _centralContext.SaveChangesAsync(cancellationToken);
                    failedTenants.Add((tenant.Id, ex.Message));
                    _logger.LogError(ex, "Error migrating tenant {TenantId}", tenant.Id);
                }
            }

            // Log summary
            if (failedTenants.Any())
            {
                _logger.LogWarning("Migration completed with {FailedCount} failures", failedTenants.Count);
                foreach (var (id, error) in failedTenants)
                {
                    _logger.LogWarning("Tenant {TenantId} failed: {Error}", id, error);
                }
                return false;
            }

            _logger.LogInformation("Successfully migrated all databases");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database migrations");
            return false;
        }
    }

    private async Task<bool> UpdateCentralDatabaseAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _centralContext.Database.MigrateAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error migrating central database");
            return false;
        }
    }

    private async Task<bool> UpdateTenantDatabaseAsync(string connectionString, CancellationToken cancellationToken)
    {
        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            await using var context = new TenantDbContext(optionsBuilder.Options, null!, _configuration);
            await context.Database.MigrateAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error migrating tenant database");
            return false;
        }
    }

    public async Task<MigrationStatus> GetMigrationStatusAsync()
    {
        try
        {
            // Check central database
            var centralPending = await _centralContext.Database.GetPendingMigrationsAsync();
            var centralApplied = await _centralContext.Database.GetAppliedMigrationsAsync();

            // Get all active tenants
            var tenants = await _centralContext.Tenants
                .Where(t => t.IsActive)
                .ToListAsync();

            var tenantStatuses = new List<TenantMigrationStatus>();

            foreach (var tenant in tenants)
            {
                try
                {
                    var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
                    optionsBuilder.UseNpgsql(tenant.ConnectionString);

                    await using var context = new TenantDbContext(optionsBuilder.Options, null!, _configuration);

                    var pending = await context.Database.GetPendingMigrationsAsync();
                    var applied = await context.Database.GetAppliedMigrationsAsync();

                    tenantStatuses.Add(new TenantMigrationStatus
                    {
                        TenantId = tenant.Id,
                        TenantName = tenant.Name,
                        PendingMigrations = pending.ToList(),
                        AppliedMigrations = applied.ToList()
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking migrations for tenant {TenantId}", tenant.Id);
                    tenantStatuses.Add(new TenantMigrationStatus
                    {
                        TenantId = tenant.Id,
                        TenantName = tenant.Name,
                        Error = ex.Message
                    });
                }
            }

            return new MigrationStatus
            {
                CentralDatabase = new DatabaseMigrationInfo
                {
                    PendingMigrations = centralPending.ToList(),
                    AppliedMigrations = centralApplied.ToList()
                },
                TenantDatabases = tenantStatuses
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting migration status");
            throw;
        }
    }
}

public class MigrationStatus
{
    public DatabaseMigrationInfo CentralDatabase { get; set; } = new();
    public List<TenantMigrationStatus> TenantDatabases { get; set; } = new();
}

public class DatabaseMigrationInfo
{
    public List<string> PendingMigrations { get; set; } = new();
    public List<string> AppliedMigrations { get; set; } = new();
}

public class TenantMigrationStatus
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public List<string> PendingMigrations { get; set; } = new();
    public List<string> AppliedMigrations { get; set; } = new();
    public string? Error { get; set; }
}