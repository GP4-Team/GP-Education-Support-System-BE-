using ESS.Application.Common.Models;
using MediatR;

namespace ESS.Application.Features.Tenants.Commands;

public record CreateTenantCommand : IRequest<Result<Guid>>
{
    public required string Name { get; init; }
    public required string Identifier { get; init; }
    public string ConnectionString { get; init; } = string.Empty; 
    public required string PrimaryDomain { get; init; }
    public bool UseSharedDatabase { get; init; }
    public Dictionary<string, string>? InitialSettings { get; init; }
}