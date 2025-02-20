public record GetMediaByCollectionQuery(Guid ResourceId, string Collection) : IRequest<IEnumerable<MediaDto>>;
