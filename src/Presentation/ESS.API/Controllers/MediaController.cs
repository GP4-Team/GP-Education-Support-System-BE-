// src/Presentation/ESS.API/Controllers/MediaController.cs
using Microsoft.AspNetCore.Mvc;
using ESS.Application.Features.Media.Commands;
using ESS.Application.Features.Media.Queries;
using ESS.Application.Features.Media.DTOs;
using ESS.Application.Common.Interfaces;

namespace ESS.API.Controllers;

public class MediaController : ApiControllerBase
{
    private readonly ILogger<MediaController> _logger;
    private readonly ITenantService _tenantService;

    public MediaController(
        ILogger<MediaController> logger,
        ITenantService tenantService)
    {
        _logger = logger;
        _tenantService = tenantService;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(52428800)] // 50MB
    [ProducesResponseType(typeof(UploadedFileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadFile(
        IFormFile file,
        [FromQuery] string collection,
        [FromHeader(Name = "X-Tenant-Id")] string tenantId,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate tenant
            if (string.IsNullOrEmpty(tenantId))
            {
                return BadRequest(new { error = "Tenant ID is required. Please provide X-Tenant-Id header." });
            }

            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded" });

            using var stream = file.OpenReadStream();
            var command = new UploadTempFileCommand
            {
                FileStream = stream,
                FileName = file.FileName,
                MimeType = file.ContentType,
                FileSize = file.Length,
                Collection = collection,
                TenantId = tenantId // Pass tenant ID to command
            };

            var result = await Mediator.Send(command, cancellationToken);
            return result.Succeeded ? Ok(result.Data) : BadRequest(new { error = result.Errors });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Tenant"))
        {
            _logger.LogWarning(ex, "Tenant error during file upload");
            return BadRequest(new { error = "Tenant not found or inactive" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", file?.FileName);
            return StatusCode(500, new { error = "Error uploading file" });
        }
    }

    [HttpPost("associate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssociateMedia(
        [FromBody] AssociateMediaCommand command,
        [FromHeader(Name = "X-Tenant-Id")] string tenantId,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate tenant
            if (string.IsNullOrEmpty(tenantId))
            {
                return BadRequest(new { error = "Tenant ID is required. Please provide X-Tenant-Id header." });
            }

            // Add tenant ID to command
            command = command with { TenantId = tenantId };

            var result = await Mediator.Send(command, cancellationToken);
            return result.Succeeded ? Ok() : BadRequest(new { error = result.Errors });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Tenant"))
        {
            _logger.LogWarning(ex, "Tenant error during media association");
            return BadRequest(new { error = "Tenant not found or inactive" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error associating media: {Error}", ex.Message);
            return StatusCode(500, new { error = "Error associating media" });
        }
    }

    [HttpGet("{resourceType}/{resourceId}")]
    [ProducesResponseType(typeof(IEnumerable<MediaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMediaByResource(
        string resourceType,
        Guid resourceId,
        [FromQuery] string? collection,
        [FromHeader(Name = "X-Tenant-Id")] string tenantId,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate tenant
            if (string.IsNullOrEmpty(tenantId))
            {
                return BadRequest(new { error = "Tenant ID is required. Please provide X-Tenant-Id header." });
            }

            var query = new GetMediaByResourceQuery
            {
                ResourceId = resourceId,
                ResourceType = resourceType,
                Collection = collection,
                TenantId = tenantId // Pass tenant ID to query
            };

            var result = await Mediator.Send(query, cancellationToken);
            return result.Succeeded ? Ok(result.Data) : BadRequest(new { error = result.Errors });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Tenant"))
        {
            _logger.LogWarning(ex, "Tenant error during media retrieval");
            return BadRequest(new { error = "Tenant not found or inactive" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving media: {Error}", ex.Message);
            return StatusCode(500, new { error = "Error retrieving media" });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMedia(
        Guid id,
        [FromHeader(Name = "X-Tenant-Id")] string tenantId,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate tenant
            if (string.IsNullOrEmpty(tenantId))
            {
                return BadRequest(new { error = "Tenant ID is required. Please provide X-Tenant-Id header." });
            }

            var command = new DeleteMediaCommand
            {
                MediaId = id,
                TenantId = tenantId // Pass tenant ID to command
            };

            var result = await Mediator.Send(command, cancellationToken);
            return result.Succeeded ? Ok() : BadRequest(new { error = result.Errors });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Tenant"))
        {
            _logger.LogWarning(ex, "Tenant error during media deletion");
            return BadRequest(new { error = "Tenant not found or inactive" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting media: {Error}", ex.Message);
            return StatusCode(500, new { error = "Error deleting media" });
        }
    }
}