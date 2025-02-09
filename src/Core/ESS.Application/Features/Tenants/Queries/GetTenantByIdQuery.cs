using ESS.Application.Common.Models;
using ESS.Application.Features.Tenants.DTOs;
using MediatR;

namespace ESS.Application.Features.Tenants.Queries;
public record GetTenantByIdQuery : IRequest<Result<TenantDto>>
{
    public required Guid Id { get; init; }
}
