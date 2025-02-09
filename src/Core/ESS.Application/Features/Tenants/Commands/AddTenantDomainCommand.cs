using MediatR;
using ESS.Application.Common.Models;
namespace ESS.Application.Features.Tenants.Commands;
public record AddTenantDomainCommand : IRequest<Result<Guid>>
{
    public required Guid TenantId { get; init; }
    public required string Domain { get; init; }
    public bool IsPrimary { get; init; }
}