public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, string>
{
    private readonly IFileUploadService _fileUploadService;

    public UploadFileCommandHandler(IFileUploadService fileUploadService)
    {
        _fileUploadService = fileUploadService;
    }

    public async Task<string> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        return await _fileUploadService.UploadAsync(request.File, request.TempFolder);
    }
}
