using ESS.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ESS.Infrastructure.Persistence;
using ESS.Domain.Entities;
using ESS.Domain.Constants;

namespace ESS.API.Controllers;

[ApiController]
[Route("api/system")]
public class SystemTestController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SystemTestController> _logger;
    private readonly IConfiguration _configuration;
    private readonly ITenantDatabaseService _tenantDatabaseService;

    public SystemTestController(
        ApplicationDbContext dbContext,
        ICacheService cacheService,
        ILogger<SystemTestController> logger,
        IConfiguration configuration,
        ITenantDatabaseService tenantDatabaseService)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
        _logger = logger;
        _configuration = configuration;
        _tenantDatabaseService = tenantDatabaseService;
    }

    [HttpGet("database/test")]
    public async Task<IActionResult> TestDatabaseConnection()
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync();
            var pendingMigrations = (await _dbContext.Database.GetPendingMigrationsAsync()).ToList();
            var connectionString = _dbContext.Database.GetConnectionString();
            var currentDatabase = _dbContext.Database.GetDbConnection().Database;

            // Safe masking of connection string
            string? maskedConnectionString = null;
            if (!string.IsNullOrEmpty(connectionString))
            {
                var password = _configuration["POSTGRES_PASSWORD"];
                maskedConnectionString = !string.IsNullOrEmpty(password)
                    ? connectionString.Replace(password, "***")
                    : connectionString.Replace("postgres", "***"); // fallback
            }

            return Ok(new
            {
                DatabaseConnection = canConnect,
                PendingMigrations = pendingMigrations,
                CurrentDatabase = currentDatabase,
                ConnectionString = maskedConnectionString ?? "Connection string not available"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing database connection");
            return Problem(
                title: "Database Connection Test Failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    [HttpPost("tenant/test")]
    public async Task<IActionResult> CreateTestTenant()
    {
        try
        {
            // Generate unique tenant details
            var identifier = $"test-{DateTime.UtcNow.Ticks}";

            // Check for existing tenant
            if (await _dbContext.Tenants.AnyAsync(t => t.Identifier == identifier))
            {
                return BadRequest(new { error = "Tenant with similar identifier already exists" });
            }

            // Default connection string with fallback
            var connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Default connection string is not configured");

            // Create tenant using the factory method
            var tenant = Tenant.Create(
                name: "Test Tenant",
                identifier: identifier,
                useSharedDatabase: false
            );

            // Update connection string
            tenant.UpdateConnectionString(connectionString);

            // Add domain using the domain method
            tenant.AddDomain($"{identifier}.example.com", isPrimary: true);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Add tenant to context
                _dbContext.Tenants.Add(tenant);
                await _dbContext.SaveChangesAsync();

                // Create tenant database
                if (!await _tenantDatabaseService.CreateTenantDatabaseAsync(tenant))
                {
                    await transaction.RollbackAsync();
                    return Problem(
                        title: "Failed to Create Tenant Database",
                        detail: "Database creation failed",
                        statusCode: StatusCodes.Status500InternalServerError
                    );
                }

                // Cache the tenant information
                var primaryDomain = tenant.Domains.First(d => d.IsPrimary);
                await _cacheService.SetAsync(
                    CacheKeys.TenantByDomain(primaryDomain.Domain),
                    tenant,
                    TimeSpan.FromHours(1));

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Created test tenant. ID: {TenantId}, Domain: {Domain}",
                    tenant.Id,
                    primaryDomain.Domain);

                return Ok(new { tenant, domain = primaryDomain });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create test tenant");
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating test tenant");
            return Problem(
                title: "Failed to Create Test Tenant",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}