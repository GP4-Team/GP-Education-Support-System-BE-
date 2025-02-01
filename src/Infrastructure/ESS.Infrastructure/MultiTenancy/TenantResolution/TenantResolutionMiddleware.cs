using ESS.Application.Common.Interfaces;
using ESS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ESS.Infrastructure.MultiTenancy.TenantResolution;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITenantResolver _tenantResolver;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ITenantResolver tenantResolver,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _tenantResolver = tenantResolver;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var host = context.Request.Host.Host;
            var tenant = await _tenantResolver.ResolveTenantAsync(host);

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