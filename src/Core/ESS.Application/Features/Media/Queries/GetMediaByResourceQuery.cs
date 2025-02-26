// src/Core/ESS.Application/Features/Media/Queries/GetMediaByResourceQuery.cs
using MediatR;
using ESS.Application.Common.Models;
using ESS.Application.Features.Media.DTOs;

namespace ESS.Application.Features.Media.Queries;

// src/Core/ESS.Application/Features/Media/Queries/GetMediaByResourceQuery.cs
public record GetMediaByResourceQuery : IRequest<Result<IEnumerable<MediaDto>>>
{
    public Guid ResourceId { get; init; }
    public string ResourceType { get; init; } = default!;
    public string? Collection { get; init; }
    public string TenantId { get; init; } = default!;
}