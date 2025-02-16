using ESS.Application.Common.Caching;
using ESS.Application.Common.Models;
using ESS.Application.Features.Tenants.DTOs;
using ESS.Domain.Constants;

namespace ESS.Application.Features.Tenants.Queries;

public record GetTenantByDomainQuery : ICachedQuery<Result<TenantDto>>
{
    public required string Domain { get; init; }

    public string CacheKey => CacheKeys.TenantByDomain(Domain);

    public TimeSpan? Expiration => TimeSpan.FromMinutes(30); // You can adjust this value
}