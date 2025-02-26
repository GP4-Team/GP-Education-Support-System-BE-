// src/Core/ESS.Application/Features/Media/Commands/UploadTempFileCommand.cs
using MediatR;
using ESS.Application.Common.Models;
using ESS.Application.Features.Media.DTOs;

namespace ESS.Application.Features.Media.Commands;

public record UploadTempFileCommand : IRequest<Result<UploadedFileDto>>
{
    public Stream FileStream { get; init; } = default!;
    public string FileName { get; init; } = default!;
    public string MimeType { get; init; } = default!;
    public long FileSize { get; init; }
    public string Collection { get; init; } = default!;
    public string TenantId { get; init; } = default!;
}