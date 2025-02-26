// src/Core/ESS.Application/Features/Media/DTOs/UploadedFileDto.cs

public record UploadedFileDto
{
    public Guid TempGuid { get; init; }
    public string FileName { get; init; } = default!;
    public string FilePath { get; init; } = default!;
}