namespace ESS.Application.Common.Caching;

public static class CacheConfiguration
{
    public static class Tenants
    {
        public const string ByDomain = "tenant-domain-{0}";
        public const string ById = "tenant-id-{0}";
        public const string Settings = "tenant-settings-{0}";
        public const string ConnectionString = "tenant-connection-{0}";
        public static TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);
    }
}
