// src/Infrastructure/ESS.Infrastructure/Media/Services/S3MediaStorageService.cs
using Microsoft.Extensions.Logging;
using Amazon.S3;
using Amazon.S3.Transfer;
using ESS.Application.Common.Interfaces;
using ESS.Infrastructure.Media.Configuration;
using Microsoft.Extensions.Options;

namespace ESS.Infrastructure.Media.Services;

public class S3MediaStorageService : IMediaStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly ILogger<S3MediaStorageService> _logger;

    public S3MediaStorageService(
        IAmazonS3 s3Client,
        IOptions<AwsS3Settings> settings,
        ILogger<S3MediaStorageService> logger)
    {
        _s3Client = s3Client;
        _bucketName = settings.Value.BucketName;
        _logger = logger;
    }

    public async Task<string> UploadTempFileAsync(
        Stream fileStream,
        string fileName,
        Guid tempGuid,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"temp/{tempGuid}/{fileName}";

            using var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(
                fileStream,
                _bucketName,
                key,
                cancellationToken);

            _logger.LogInformation("Successfully uploaded file to S3: {Key}", key);
            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to S3: {FileName}", fileName);
            throw;
        }
    }

    public async Task MoveFileAsync(
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Copy object
            await _s3Client.CopyObjectAsync(
                _bucketName,
                sourcePath,
                _bucketName,
                destinationPath,
                cancellationToken);

            // Delete original
            await _s3Client.DeleteObjectAsync(
                _bucketName,
                sourcePath,
                cancellationToken);

            _logger.LogInformation(
                "Successfully moved file in S3 from {Source} to {Destination}",
                sourcePath,
                destinationPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error moving file in S3 from {Source} to {Destination}",
                sourcePath,
                destinationPath);
            throw;
        }
    }

    public async Task DeleteFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _s3Client.DeleteObjectAsync(
                _bucketName,
                filePath,
                cancellationToken);

            _logger.LogInformation("Successfully deleted file from S3: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from S3: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<Stream> GetFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _s3Client.GetObjectAsync(
                _bucketName,
                filePath,
                cancellationToken);

            return response.ResponseStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file from S3: {FilePath}", filePath);
            throw;
        }
    }
}