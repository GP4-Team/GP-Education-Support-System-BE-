using ESS.Domain.Entities;

namespace ESS.Application.Common.Interfaces;

public interface ITenantDatabaseService
{
    Task<bool> CreateTenantDatabaseAsync(Tenant tenant);
    Task<bool> ValidateDatabaseConnectionAsync(string connectionString);
    Task<bool> CheckDatabaseExistsAsync(string databaseName);
}