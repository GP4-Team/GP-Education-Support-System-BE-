public class Media
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ResourceId { get; private set; }
    public string FilePath { get; private set; }
    public string Collection { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public Media(Guid resourceId, string filePath, string collection)
    {
        ResourceId = resourceId;
        FilePath = filePath;
        Collection = collection;
    }
}
