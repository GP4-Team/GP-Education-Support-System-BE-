using ESS.Application.Common.Interfaces;
using ESS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ESS.Infrastructure.MultiTenancy.TenantResolution;

public class TenantResolutionMiddleware
{
    private const string TenantIdHeader = "X-Tenant-ID";
    private const string TenantIdentifierHeader = "X-Tenant-Identifier";
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private static readonly string[] ExcludedPaths = new[]
    {
        "/health",
        "/healthz",
        "/swagger",
        "/.well-known",
        "/api/tenants" // Exclude the tenant creation endpoint
    };

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> logger,
        IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();

        // Skip tenant resolution for excluded paths
        if (path != null &&
            (ExcludedPaths.Any(excluded => path.StartsWith(excluded, StringComparison.OrdinalIgnoreCase)) ||
             IsTenantsApiManagementEndpoint(path, context.Request.Method)))
        {
            await _next(context);
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();

            // Try to resolve tenant from headers first
            var tenant = await ResolveTenantFromHeadersAsync(context, tenantService);

            // If no tenant found from headers, try domain resolution
            if (tenant == null)
            {
                var tenantResolver = scope.ServiceProvider.GetRequiredService<ITenantResolver>();
                var host = context.Request.Host.Host;
                tenant = await tenantResolver.ResolveTenantAsync(host);
            }

            if (tenant != null)
            {
                await HandleValidTenantAsync(context, tenant);
            }
            else
            {
                await HandleTenantNotFoundAsync(context);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing tenant resolution");
            throw;
        }
    }

    private bool IsTenantsApiManagementEndpoint(string path, string method)
    {
        // Exact match for POST /api/tenants (create tenant)
        if (path == "/api/tenants" && method == "POST")
            return true;

        // Match for GET /api/tenants to list all tenants
        if (path == "/api/tenants" && method == "GET")
            return true;

        // Match for tenant migrations endpoints
        if (path.StartsWith("/api/tenant-migrations", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    private async Task<Tenant?> ResolveTenantFromHeadersAsync(HttpContext context, ITenantService tenantService)
    {
        // Try to get tenant by ID first
        if (context.Request.Headers.TryGetValue(TenantIdHeader, out var tenantId) &&
            Guid.TryParse(tenantId, out var parsedTenantId))
        {
            var tenant = await tenantService.GetTenantByIdAsync(parsedTenantId);
            if (tenant != null)
            {
                _logger.LogInformation("Tenant resolved from header ID: {TenantId}", parsedTenantId);
                return tenant;
            }
        }

        // Try to get tenant by identifier
        if (context.Request.Headers.TryGetValue(TenantIdentifierHeader, out var identifier) &&
            !string.IsNullOrEmpty(identifier))
        {
            var tenant = await tenantService.GetTenantByIdentifierAsync(identifier!);
            if (tenant != null)
            {
                _logger.LogInformation("Tenant resolved from header identifier: {Identifier}", identifier.ToString());
                return tenant;
            }
        }

        return null;
    }

    private async Task HandleValidTenantAsync(HttpContext context, Tenant tenant)
    {
        if (!tenant.IsActive)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "Tenant is inactive" }));
            return;
        }

        // Use the new extension method
        context.SetCurrentTenant(tenant);

        if (!context.Response.HasStarted)
        {
            context.Response.Headers[TenantIdHeader] = tenant.Id.ToString();
            context.Response.Headers[TenantIdentifierHeader] = tenant.Identifier;
        }

        await _next(context);
    }

    private async Task HandleTenantNotFoundAsync(HttpContext context)
    {
        _logger.LogWarning("No tenant found for request: {Host}", context.Request.Host.Host);
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "Tenant not found" }));
    }
}