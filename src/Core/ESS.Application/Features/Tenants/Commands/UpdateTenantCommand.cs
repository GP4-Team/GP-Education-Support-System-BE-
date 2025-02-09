
using ESS.Application.Common.Models;
using MediatR;
namespace ESS.Application.Features.Tenants.Commands;
public record UpdateTenantCommand : IRequest<Result<Unit>>
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required bool IsActive { get; init; }
}
