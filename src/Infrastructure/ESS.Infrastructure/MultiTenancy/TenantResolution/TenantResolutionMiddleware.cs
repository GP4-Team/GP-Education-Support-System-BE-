using ESS.Application.Common.Interfaces;
using ESS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ESS.Infrastructure.MultiTenancy.TenantResolution;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private static readonly string[] ExcludedPaths = new[]
    {
        "/health",
        "/healthz",
        "/swagger",
        "/.well-known"
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
        // Check if the path should be excluded from tenant resolution
        var path = context.Request.Path.Value?.ToLowerInvariant();
        if (path != null && ExcludedPaths.Any(excluded => path.StartsWith(excluded, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var tenantResolver = scope.ServiceProvider.GetRequiredService<ITenantResolver>();

            var host = context.Request.Host.Host;
            var tenant = await tenantResolver.ResolveTenantAsync(host);

            if (tenant != null)
            {
                context.Items["CurrentTenant"] = tenant;

                if (!context.Response.HasStarted)
                {
                    context.Response.Headers["X-Tenant-Id"] = tenant.Id.ToString();
                }

                await _next(context);
            }
            else
            {
                _logger.LogWarning("No tenant found for host: {Host}", host);
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";

                var error = new { error = "Tenant not found" };
                await context.Response.WriteAsync(JsonSerializer.Serialize(error));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing tenant resolution");
            throw;
        }
    }
}