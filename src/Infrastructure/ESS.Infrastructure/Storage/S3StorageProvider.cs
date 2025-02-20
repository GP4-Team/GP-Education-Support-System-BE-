public class S3StorageProvider : IStorageProvider
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3StorageProvider(IAmazonS3 s3Client, IConfiguration config)
    {
        _s3Client = s3Client;
        _bucketName = config["AmazonS3:BucketName"];
    }

    public async Task<string> SaveFileAsync(IFormFile file, string path)
    {
        using var stream = file.OpenReadStream();
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = path,
            InputStream = stream,
            ContentType = file.ContentType
        };
        await _s3Client.PutObjectAsync(request);
        return path;
    }

    public async Task MoveDirectoryAsync(string sourcePath, string destPath)
    {
        var files = await _s3Client.ListObjectsV2Async(new ListObjectsV2Request
        {
            BucketName = _bucketName,
            Prefix = sourcePath
        });

        foreach (var file in files.S3Objects)
        {
            var newKey = file.Key.Replace(sourcePath, destPath);
            await _s3Client.CopyObjectAsync(_bucketName, file.Key, _bucketName, newKey);
            await _s3Client.DeleteObjectAsync(_bucketName, file.Key);
        }
    }

    public async Task<string> GetPresignedUrlAsync(string filePath)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = filePath,
            Expires = DateTime.UtcNow.AddMinutes(15)
        };
        return _s3Client.GetPreSignedURL(request);
    }
}
