public interface IMediaRepository
{
    Task<IEnumerable<Media>> GetByResourceAsync(Guid resourceId);
    Task AddAsync(Media media);
}
