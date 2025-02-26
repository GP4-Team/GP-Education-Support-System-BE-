// src/Infrastructure/ESS.Infrastructure/Media/Configuration/MediaValidationSettings.cs

namespace ESS.Infrastructure.Media.Configuration;

public class MediaValidationSettings
{
    public long MaxFileSize { get; set; }
    public string[] AllowedFileTypes { get; set; } = Array.Empty<string>();
}