using ESS.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace ESS.Infrastructure.MultiTenancy;

public class TenantContextAccessor : ITenantContextAccessor
{
    private static readonly AsyncLocal<TenantContextHolder> _tenantContext = new();
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContextAccessor(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public Tenant? CurrentTenant
    {
        get
        {
            // First try AsyncLocal
            if (_tenantContext.Value?.Tenant != null)
            {
                return _tenantContext.Value.Tenant;
            }

            // Then try HttpContext
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Items != null &&
                httpContext.Items.TryGetValue("CurrentTenant", out var tenant) &&
                tenant is Tenant currentTenant)
            {
                return currentTenant;
            }

            return null;
        }
    }

    public void SetCurrentTenant(Tenant tenant)
    {
        if (tenant == null)
        {
            throw new ArgumentNullException(nameof(tenant));
        }

        // Set in AsyncLocal
        _tenantContext.Value = new TenantContextHolder { Tenant = tenant };

        // Also set in HttpContext if available
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Items != null)
        {
            httpContext.Items["CurrentTenant"] = tenant;
        }
    }

    public void ClearCurrentTenant()
    {
        _tenantContext.Value = new TenantContextHolder { Tenant = null };

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Items != null && httpContext.Items.ContainsKey("CurrentTenant"))
        {
            httpContext.Items.Remove("CurrentTenant");
        }
    }

    private class TenantContextHolder
    {
        public Tenant? Tenant { get; set; }
    }
}