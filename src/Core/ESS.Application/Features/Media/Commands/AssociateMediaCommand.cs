// src/Core/ESS.Application/Features/Media/Commands/AssociateMediaCommand.cs
using MediatR;
using ESS.Application.Common.Models;

namespace ESS.Application.Features.Media.Commands;

// src/Core/ESS.Application/Features/Media/Commands/AssociateMediaCommand.cs
public record AssociateMediaCommand : IRequest<Result<Unit>>
{
    public Guid TempGuid { get; init; }
    public Guid ResourceId { get; init; }
    public string ResourceType { get; init; } = default!;
    public string Collection { get; init; } = default!;
    public string TenantId { get; init; } = default!;
}