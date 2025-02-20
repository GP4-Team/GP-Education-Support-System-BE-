public interface IStorageProvider
{
    Task<string> SaveFileAsync(IFormFile file, string path);
    Task MoveDirectoryAsync(string sourcePath, string destPath);
    Task<string> GetPresignedUrlAsync(string filePath);
}
