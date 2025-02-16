using ESS.Application.Common.Interfaces;
using ESS.Domain.Entities;
using ESS.Domain.Enums;
using ESS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ESS.Infrastructure.Services;

public class TenantMigrationService : ITenantMigrationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TenantMigrationService> _logger;
    private readonly IApplicationDbContext _context;
    private readonly string _masterConnectionString;

    public TenantMigrationService(
        IConfiguration configuration,
        ILogger<TenantMigrationService> logger,
        IApplicationDbContext context)
    {
        _configuration = configuration;
        _logger = logger;
        _context = context;
        _masterConnectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<bool> InitializeTenantDatabaseAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        try
        {
            tenant.SetDatabaseStatus(TenantDatabaseStatus.Creating);
            await _context.SaveChangesAsync(cancellationToken);

            if (!await CreateTenantDatabaseFromTemplateAsync(tenant.ConnectionString!, cancellationToken))
            {
                tenant.SetDatabaseStatus(TenantDatabaseStatus.Failed, "Failed to create database from template");
                await _context.SaveChangesAsync(cancellationToken);
                return false;
            }

            if (!await MigrateTenantDatabaseAsync(tenant.ConnectionString!, cancellationToken))
            {
                tenant.SetDatabaseStatus(TenantDatabaseStatus.Failed, "Failed to apply migrations");
                await _context.SaveChangesAsync(cancellationToken);
                return false;
            }

            tenant.SetDatabaseStatus(TenantDatabaseStatus.Active);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize database for tenant {TenantId}", tenant.Id);
            tenant.SetDatabaseStatus(TenantDatabaseStatus.Failed, ex.Message);
            await _context.SaveChangesAsync(cancellationToken);
            return false;
        }
    }

    public async Task<bool> MigrateTenantDatabaseAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            using var context = new TenantDbContext(optionsBuilder.Options, null!, _configuration);
            await context.Database.MigrateAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to migrate database");
            return false;
        }
    }

    public async Task<bool> ValidateTenantDatabaseAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            using var context = new TenantDbContext(optionsBuilder.Options, null!, _configuration);

            if (!await context.Database.CanConnectAsync(cancellationToken))
                return false;

            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
            return !pendingMigrations.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate database");
            return false;
        }
    }

    public async Task<bool> CreateTenantDatabaseFromTemplateAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            string databaseName = builder.Database;

            using var masterConnection = new NpgsqlConnection(_masterConnectionString);
            await masterConnection.OpenAsync(cancellationToken);

            // Check if database exists
            using (var command = new NpgsqlCommand(
                "SELECT 1 FROM pg_database WHERE datname = @dbname",
                masterConnection))
            {
                command.Parameters.AddWithValue("@dbname", databaseName);
                if (await command.ExecuteScalarAsync(cancellationToken) != null)
                {
                    return true;
                }
            }

            // Create database from template
            using (var command = new NpgsqlCommand(
                $"CREATE DATABASE \"{databaseName}\" TEMPLATE template0",
                masterConnection))
            {
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create database from template");
            return false;
        }
    }

    public async Task<bool> BackupTenantDatabaseAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(tenant.ConnectionString);
            string databaseName = builder.Database;
            string backupPath = Path.Combine("Backups", $"{databaseName}_{DateTime.UtcNow:yyyyMMddHHmmss}.dump");

            Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);

            using var masterConnection = new NpgsqlConnection(_masterConnectionString);
            await masterConnection.OpenAsync(cancellationToken);

            using var command = new NpgsqlCommand(
                $"pg_dump -Fc -f {backupPath} {databaseName}",
                masterConnection);
            await command.ExecuteNonQueryAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to backup database for tenant {TenantId}", tenant.Id);
            return false;
        }
    }

    public async Task<bool> RestoreTenantDatabaseAsync(string connectionString, string backupPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            string databaseName = builder.Database;

            using var masterConnection = new NpgsqlConnection(_masterConnectionString);
            await masterConnection.OpenAsync(cancellationToken);

            using var command = new NpgsqlCommand(
                $"pg_restore -d {databaseName} {backupPath}",
                masterConnection);
            await command.ExecuteNonQueryAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore database");
            return false;
        }
    }
}