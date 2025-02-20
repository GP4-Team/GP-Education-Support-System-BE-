public class FileUploadService : IFileUploadService
{
    private readonly IStorageProvider _storageProvider;

    public FileUploadService(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    public async Task<string> UploadAsync(IFormFile file, string tempFolder)
    {
        var filePath = Path.Combine("temp", tempFolder, file.FileName);
        return await _storageProvider.SaveFileAsync(file, filePath);
    }

    public async Task MoveFilesToResourceAsync(string folderGuid, Guid resourceId, string collection)
    {
        var tempPath = Path.Combine("temp", folderGuid);
        var destPath = Path.Combine("storage", resourceId.ToString(), collection);

        await _storageProvider.MoveDirectoryAsync(tempPath, destPath);
    }
}
