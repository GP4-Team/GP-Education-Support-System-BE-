public record UploadFileCommand(IFormFile File, string TempFolder) : IRequest<string>;
