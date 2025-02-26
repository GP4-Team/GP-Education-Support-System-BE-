// src/Core/ESS.Application/Features/Media/Commands/DeleteMediaCommand.cs

using MediatR;
using ESS.Application.Common.Models;


// src/Core/ESS.Application/Features/Media/Commands/DeleteMediaCommand.cs
public record DeleteMediaCommand : IRequest<Result<Unit>>
{
    public Guid MediaId { get; init; }
    public string TenantId { get; init; } = default!;
}