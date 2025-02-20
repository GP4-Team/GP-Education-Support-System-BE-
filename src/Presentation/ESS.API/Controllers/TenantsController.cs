using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ESS.Application.Common.Models;
using ESS.Application.Features.Tenants.Commands;
using ESS.Application.Features.Tenants.Queries;
using ESS.Application.Features.Tenants.DTOs;

namespace ESS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(
        IMediator mediator,
        ILogger<TenantsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all tenants
    /// </summary>
    /// <returns>List of all tenants</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TenantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllTenants()
    {
        try
        {
            var result = await _mediator.Send(new GetAllTenantsQuery());
            return result.IsSuccess
                ? Ok(result.Value)
                : StatusCode(500, "Failed to retrieve tenants");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all tenants");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Get tenant by ID
    /// </summary>
    /// <param name="id">Tenant ID</param>
    /// <returns>Tenant details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTenantById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetTenantByIdQuery { Id = id });
            return result.IsSuccess
                ? Ok(result.Value)
                : NotFound(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenant with ID {TenantId}", id);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Get tenant by identifier
    /// </summary>
    /// <param name="identifier">Tenant identifier</param>
    /// <returns>Tenant details</returns>
    [HttpGet("by-identifier/{identifier}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTenantByIdentifier(string identifier)
    {
        try
        {
            var result = await _mediator.Send(new GetTenantByIdentifierQuery { Identifier = identifier });
            return result.IsSuccess
                ? Ok(result.Value)
                : NotFound(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenant with identifier {Identifier}", identifier);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Get tenant by domain
    /// </summary>
    /// <param name="domain">Domain name</param>
    /// <returns>Tenant details</returns>
    [HttpGet("by-domain/{domain}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTenantByDomain(string domain)
    {
        try
        {
            var result = await _mediator.Send(new GetTenantByDomainQuery { Domain = domain });
            return result.IsSuccess
                ? Ok(result.Value)
                : NotFound(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenant with domain {Domain}", domain);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Create a new tenant
    /// </summary>
    /// <remarks>
    /// If no connection string is provided, one will be automatically generated based on the central database configuration.
    /// The generated connection string will create a new database named 'tenant_{identifier}'.
    /// </remarks>
    /// <param name="command">Create tenant command</param>
    /// <returns>ID of the created tenant</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantCommand command)
    {
        try
        {
            // If no connection string provided, generate one automatically
            if (string.IsNullOrWhiteSpace(command.ConnectionString))
            {
                var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                command = command with
                {
                    ConnectionString = ESS.API.Utilities.PostgresConnectionStringGenerator.GenerateConnectionString(
                        configuration,
                        command.Identifier)
                };
                _logger.LogInformation("Generated connection string for tenant {Identifier}", command.Identifier);
            }
            else
            {
                // Validate provided connection string
                var (isValid, errorMessage) = ESS.API.Utilities.PostgresConnectionStringValidator.ValidateConnectionString(command.ConnectionString);
                if (!isValid)
                {
                    _logger.LogWarning("Invalid connection string provided: {Error}", errorMessage);
                    return BadRequest(new
                    {
                        error = $"Invalid connection string: {errorMessage}",
                        sampleFormat = ESS.API.Utilities.PostgresConnectionStringValidator.GetSampleConnectionString()
                    });
                }
            }

            var result = await _mediator.Send(command);

            return result.IsSuccess
                ? CreatedAtAction(nameof(GetTenantById), new { id = result.Value }, result.Value)
                : BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Update a tenant's details
    /// </summary>
    /// <param name="id">Tenant ID</param>
    /// <param name="command">Update tenant command</param>
    /// <returns>No content on success</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTenant(Guid id, [FromBody] UpdateTenantCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest("ID in URL doesn't match ID in request body");
            }

            var result = await _mediator.Send(command);

            return result.IsSuccess
                ? NoContent()
                : NotFound(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant {TenantId}", id);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Delete a tenant
    /// </summary>
    /// <param name="id">Tenant ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTenant(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteTenantCommand { Id = id });

            return result.IsSuccess
                ? NoContent()
                : NotFound(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tenant {TenantId}", id);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Update a tenant's connection string
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="command">Update connection string command</param>
    /// <returns>No content on success</returns>
    [HttpPut("{tenantId:guid}/connection-string")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTenantConnectionString(Guid tenantId, [FromBody] UpdateTenantConnectionStringCommand command)
    {
        try
        {
            if (tenantId != command.TenantId)
            {
                return BadRequest("Tenant ID in URL doesn't match ID in request body");
            }

            // Validate connection string format
            var (isValid, errorMessage) = ESS.API.Utilities.PostgresConnectionStringValidator.ValidateConnectionString(command.ConnectionString);
            if (!isValid)
            {
                _logger.LogWarning("Invalid connection string provided: {Error}", errorMessage);
                return BadRequest(new
                {
                    error = $"Invalid connection string: {errorMessage}",
                    sampleFormat = ESS.API.Utilities.PostgresConnectionStringValidator.GetSampleConnectionString()
                });
            }

            var result = await _mediator.Send(command);

            return result.IsSuccess
                ? NoContent()
                : result.Error.Contains("not found")
                    ? NotFound(result.Error)
                    : BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating connection string for tenant {TenantId}", tenantId);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Add a domain to a tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="command">Add domain command</param>
    /// <returns>ID of the created domain</returns>
    [HttpPost("{tenantId:guid}/domains")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddTenantDomain(Guid tenantId, [FromBody] AddTenantDomainCommand command)
    {
        try
        {
            if (tenantId != command.TenantId)
            {
                return BadRequest("Tenant ID in URL doesn't match ID in request body");
            }

            var result = await _mediator.Send(command);

            return result.IsSuccess
                ? CreatedAtAction(nameof(GetTenantById), new { id = tenantId }, result.Value)
                : result.Error.Contains("not found")
                    ? NotFound(result.Error)
                    : BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding domain to tenant {TenantId}", tenantId);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Update tenant settings
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="command">Update settings command</param>
    /// <returns>No content on success</returns>
    [HttpPut("{tenantId:guid}/settings")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTenantSettings(Guid tenantId, [FromBody] UpdateTenantSettingsCommand command)
    {
        try
        {
            if (tenantId != command.TenantId)
            {
                return BadRequest("Tenant ID in URL doesn't match ID in request body");
            }

            var result = await _mediator.Send(command);

            return result.IsSuccess
                ? NoContent()
                : result.Error.Contains("not found")
                    ? NotFound(result.Error)
                    : BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating settings for tenant {TenantId}", tenantId);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
}