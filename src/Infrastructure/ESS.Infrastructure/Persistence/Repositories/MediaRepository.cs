public class MediaRepository : IMediaRepository
{
    private readonly TenantDbContext _context;

    public MediaRepository(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Media>> GetByResourceAsync(Guid resourceId)
    {
        return await _context.Media.Where(m => m.ResourceId == resourceId).ToListAsync();
    }

    public async Task AddAsync(Media media)
    {
        await _context.Media.AddAsync(media);
        await _context.SaveChangesAsync();
    }
}
