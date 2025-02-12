namespace ESS.Application.Common.Caching;

public interface ICacheKey
{
    string CacheKey { get; }
    TimeSpan? Expiration { get; }
}
