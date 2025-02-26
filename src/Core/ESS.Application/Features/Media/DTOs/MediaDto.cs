// src/Core/ESS.Application/Features/Media/DTOs/MediaDto.cs

namespace ESS.Application.Features.Media.DTOs;

public record MediaDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = default!;
    public string FileType { get; init; } = default!;
    public string MimeType { get; init; } = default!;
    public long Size { get; init; }
    public string FilePath { get; init; } = default!;
    public string Collection { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
}