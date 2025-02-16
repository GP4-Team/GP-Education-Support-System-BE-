using ESS.Application.Common.Models;
using MediatR;

namespace ESS.Application.Features.Tenants.Commands;

public record UpdateTenantConnectionStringCommand : IRequest<Result>
{
    public Guid TenantId { get; init; }
    public string ConnectionString { get; init; } = string.Empty;
}