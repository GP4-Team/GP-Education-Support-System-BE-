public interface IFileUploadService
{
	Task<string> UploadAsync(IFormFile file, string tempFolder);
	Task MoveFilesToResourceAsync(string folderGuid, Guid resourceId, string collection);
}
