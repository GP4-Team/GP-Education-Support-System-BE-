public class GetMediaByCollectionQueryHandler : IRequestHandler<GetMediaByCollectionQuery, IEnumerable<MediaDto>>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IStorageProvider _storageProvider;

    public GetMediaByCollectionQueryHandler(IMediaRepository mediaRepository, IStorageProvider storageProvider)
    {
        _mediaRepository = mediaRepository;
        _storageProvider = storageProvider;
    }

    public async Task<IEnumerable<MediaDto>> Handle(GetMediaByCollectionQuery request, CancellationToken cancellationToken)
    {
        var mediaFiles = await _mediaRepository.GetByResourceAsync(request.ResourceId);
        var filteredMedia = mediaFiles.Where(m => m.Collection == request.Collection);

        return filteredMedia.Select(m => new MediaDto(m.Id, _storageProvider.GetPresignedUrlAsync(m.FilePath).Result));
    }
}
