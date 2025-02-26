// src/Infrastructure/ESS.Infrastructure/Media/Configuration/AwsS3Settings.cs

namespace ESS.Infrastructure.Media.Configuration;

public class AwsS3Settings
{
    public string AccessKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public string Region { get; set; } = default!;
    public string BucketName { get; set; } = default!;
}