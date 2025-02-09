using ESS.Application.Common.Models;
using ESS.Application.Features.Tenants.DTOs;
using MediatR;

namespace ESS.Application.Features.Tenants.Queries;

public record GetTenantByDomainQuery : IRequest<Result<TenantDto>>
{
    public required string Domain { get; init; }
}