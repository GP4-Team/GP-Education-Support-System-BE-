using ESS.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ESS.Infrastructure.Persistence;
using ESS.Domain.Entities;

namespace ESS.API.Controllers;

[ApiController]
[Route("api/system")]
public class SystemTestController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SystemTestController> _logger;
    private readonly IConfiguration _configuration;

    public SystemTestController(
        ApplicationDbContext dbContext,
        ICacheService cacheService,
        ILogger<SystemTestController> logger,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
        _logger = logger;
        _configuration = configuration;
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
            var tenantId = Guid.NewGuid();
            var identifier = $"test-{DateTime.UtcNow.Ticks}";

            // Check for existing tenant
            if (await _dbContext.Tenants.AnyAsync(t => t.Identifier == identifier))
            {
                return BadRequest(new { error = "Tenant with similar identifier already exists" });
            }

            // Default connection string with fallback
            var connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Default connection string is not configured");

            var tenant = new Tenant
            {
                Id = tenantId,
                Name = "Test Tenant",
                Identifier = identifier,
                ConnectionString = connectionString,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var domain = new TenantDomain
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Domain = $"{identifier}.example.com",
                IsPrimary = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                _dbContext.Tenants.Add(tenant);
                _dbContext.TenantDomains.Add(domain);
                await _dbContext.SaveChangesAsync();

                // Cache the tenant information
                await _cacheService.SetAsync(
                    $"tenant_domain_{domain.Domain}",
                    tenant,  // Store the whole tenant object instead of just the ID
                    TimeSpan.FromHours(1));

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Created test tenant. ID: {TenantId}, Domain: {Domain}",
                    tenant.Id,
                    domain.Domain);

                return Ok(new { tenant, domain });
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