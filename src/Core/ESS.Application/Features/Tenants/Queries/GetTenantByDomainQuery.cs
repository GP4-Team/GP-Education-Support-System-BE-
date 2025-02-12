using ESS.Application.Common.Caching;
using ESS.Application.Common.Models;
using ESS.Application.Features.Tenants.DTOs;

namespace ESS.Application.Features.Tenants.Queries;

public record GetTenantByDomainQuery : ICachedQuery<Result<TenantDto>>
{
    public string Domain { get; init; }

    public string CacheKey => string.Format(CacheConfiguration.Tenants.ByDomain, Domain);
    public TimeSpan? Expiration => CacheConfiguration.Tenants.DefaultExpiration;
}
