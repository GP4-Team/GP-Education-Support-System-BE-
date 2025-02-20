public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<IFileUploadService, FileUploadService>();
        services.AddTransient<IMediaService, MediaService>();
        services.AddTransient<IMediaRepository, MediaRepository>();
        services.AddTransient<IStorageProvider, S3StorageProvider>();

        services.AddHostedService<CleanupTemporaryFilesJob>();

        return services;
    }
}
