using ESS.Domain.Entities;

namespace ESS.Application.Common.Interfaces;

public interface ITenantMigrationService
{
    Task<bool> InitializeTenantDatabaseAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task<bool> MigrateTenantDatabaseAsync(string connectionString, CancellationToken cancellationToken = default);
    Task<bool> ValidateTenantDatabaseAsync(string connectionString, CancellationToken cancellationToken = default);
    Task<bool> BackupTenantDatabaseAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task<bool> RestoreTenantDatabaseAsync(string connectionString, string backupPath, CancellationToken cancellationToken = default);
}