
using ESS.Application.Common.Models;
using MediatR;
namespace ESS.Application.Features.Tenants.Commands;
public record UpdateTenantSettingsCommand : IRequest<Result<Unit>>
{
    public required Guid TenantId { get; init; }
    public required Dictionary<string, string> Settings { get; init; }
}