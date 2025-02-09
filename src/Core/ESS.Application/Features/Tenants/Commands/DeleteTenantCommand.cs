using ESS.Application.Common.Models;
using MediatR;
namespace ESS.Application.Features.Tenants.Commands;
public record DeleteTenantCommand : IRequest<Result<Unit>>
{
    public required Guid Id { get; init; }
}

