using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ESS.Infrastructure.Services;

namespace ESS.API.Controllers;

[ApiController]
[Route("api/tenant-migrations")]
public class TenantMigrationController : ControllerBase
{
    private readonly DatabaseMigrationService _migrationService;
    private readonly ILogger<TenantMigrationController> _logger;

    public TenantMigrationController(
        DatabaseMigrationService migrationService,
        ILogger<TenantMigrationController> logger)
    {
        _migrationService = migrationService;
        _logger = logger;
    }

    /// <summary>
    /// Get migration status for all databases
    /// </summary>
    /// <returns>Migration status details</returns>
    [HttpGet("status")]
    [ProducesResponseType(typeof(MigrationStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMigrationStatus()
    {
        try
        {
            var status = await _migrationService.GetMigrationStatusAsync();
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting migration status");
            return StatusCode(500, "An error occurred while retrieving migration status");
        }
    }

    /// <summary>
    /// Run migrations for all databases
    /// </summary>
    /// <returns>Success indicator</returns>
    [HttpPost("update-all")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAllDatabases()
    {
        try
        {
            var result = await _migrationService.UpdateAllDatabasesAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating all databases");
            return StatusCode(500, "An error occurred while updating databases");
        }
    }
}