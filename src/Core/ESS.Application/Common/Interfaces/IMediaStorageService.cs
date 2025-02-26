// src/Core/ESS.Application/Common/Interfaces/IMediaStorageService.cs

namespace ESS.Application.Common.Interfaces;

public interface IMediaStorageService
{
    Task<string> UploadTempFileAsync(
        Stream fileStream,
        string fileName,
        Guid tempGuid,
        CancellationToken cancellationToken = default);

    Task MoveFileAsync(
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken = default);

    Task DeleteFileAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    Task<Stream> GetFileAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}