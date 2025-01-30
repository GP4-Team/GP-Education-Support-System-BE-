using ESS.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ESS.Infrastructure.Persistence;
using ESS.Domain.Entities;

namespace ESS.API.Controllers;

[ApiController]
[Route("api/system")]  // More specific route
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
            var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync();
            var connectionString = _dbContext.Database.GetConnectionString();

            // Mask sensitive information
            var maskedConnectionString = connectionString?.Replace(_configuration["POSTGRES_PASSWORD"] ?? "postgres", "***");

            return Ok(new
            {
                DatabaseConnection = canConnect,
                PendingMigrations = pendingMigrations.ToList(),
                CurrentDatabase = _dbContext.Database.GetDbConnection().Database,
                ConnectionString = maskedConnectionString
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
            // Validate if tenant with similar identifier already exists
            var tenantId = Guid.NewGuid();
            var identifier = $"test-{DateTime.UtcNow.Ticks}";

            if (await _dbContext.Tenants.AnyAsync(t => t.Identifier == identifier))
            {
                return BadRequest(new { error = "Tenant with similar identifier already exists" });
            }

            var tenant = new Tenant
            {
                Id = tenantId,
                Name = "Test Tenant",
                Identifier = identifier,
                ConnectionString = _configuration.GetConnectionString("DefaultConnection"),
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
                    tenant.Id,
                    TimeSpan.FromHours(1));

                await transaction.CommitAsync();
                return Ok(new { tenant, domain });
            }
            catch
            {
                await transaction.RollbackAsync();
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