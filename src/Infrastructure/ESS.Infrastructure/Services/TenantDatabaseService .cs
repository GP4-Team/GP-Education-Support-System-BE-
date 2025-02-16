using ESS.Application.Common.Interfaces;
using ESS.Domain.Entities;
using ESS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ESS.Infrastructure.Services;

public class TenantDatabaseService : ITenantDatabaseService
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ICacheService _cacheService;
    private readonly ILogger<TenantDatabaseService> _logger;
    private readonly string _masterConnectionString;

    public TenantDatabaseService(
        IApplicationDbContext context,
        IConfiguration configuration,
        ICacheService cacheService,
        ILogger<TenantDatabaseService> logger)
    {
        _context = context;
        _configuration = configuration;
        _cacheService = cacheService;
        _logger = logger;
        _masterConnectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<bool> CreateTenantDatabaseAsync(Tenant tenant)
    {
        try
        {
            // Update status to Creating
            tenant.SetDatabaseStatus(TenantDatabaseStatus.Creating);
            await _context.SaveChangesAsync(CancellationToken.None);

            // Extract database name from connection string
            var builder = new NpgsqlConnectionStringBuilder(tenant.ConnectionString);
            string databaseName = builder.Database;

            // Check if database already exists
            if (await CheckDatabaseExistsAsync(databaseName))
            {
                _logger.LogWarning("Database {DatabaseName} already exists for tenant {TenantId}", databaseName, tenant.Id);
                tenant.SetDatabaseStatus(TenantDatabaseStatus.Active);
                await _context.SaveChangesAsync(CancellationToken.None);
                return true;
            }

            // Create database using master connection
            using (var masterConnection = new NpgsqlConnection(_masterConnectionString))
            {
                await masterConnection.OpenAsync();

                // Create database
                string createDbCommand = $"CREATE DATABASE \"{databaseName}\" WITH OWNER = postgres ENCODING = 'UTF8';";
                using (var command = new NpgsqlCommand(createDbCommand, masterConnection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }

            // Validate the new database connection
            if (await ValidateDatabaseConnectionAsync(tenant.ConnectionString))
            {
                tenant.SetDatabaseStatus(TenantDatabaseStatus.Active);
                await _context.SaveChangesAsync(CancellationToken.None);
                return true;
            }

            throw new Exception("Database created but connection validation failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create database for tenant {TenantId}", tenant.Id);
            tenant.SetDatabaseStatus(TenantDatabaseStatus.Failed, ex.Message);
            await _context.SaveChangesAsync(CancellationToken.None);
            return false;
        }
    }

    public async Task<bool> ValidateDatabaseConnectionAsync(string connectionString)
    {
        try
        {
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate database connection");
            return false;
        }
    }

    public async Task<bool> CheckDatabaseExistsAsync(string databaseName)
    {
        try
        {
            using var masterConnection = new NpgsqlConnection(_masterConnectionString);
            await masterConnection.OpenAsync();

            string checkDbCommand = "SELECT 1 FROM pg_database WHERE datname = @dbname";
            using var command = new NpgsqlCommand(checkDbCommand, masterConnection);
            command.Parameters.AddWithValue("@dbname", databaseName);

            var result = await command.ExecuteScalarAsync();
            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check database existence for {DatabaseName}", databaseName);
            return false;
        }
    }
}