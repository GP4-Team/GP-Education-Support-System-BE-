using Amazon;
using Amazon.S3;
using Amazon.Runtime;
using Amazon.Extensions.NETCore.Setup;
using ESS.Application.Common.Interfaces;
using ESS.Infrastructure.Caching;
using ESS.Infrastructure.DomainEvents;
using ESS.Infrastructure.MultiTenancy.TenantResolution;
using ESS.Infrastructure.Persistence;
using ESS.Infrastructure.Services;
using ESS.Infrastructure.Configuration;
using ESS.Infrastructure.Media.Services;
using ESS.Infrastructure.Media.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ESS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Register IApplicationDbContext
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // Redis Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["Redis:Configuration"];
            options.InstanceName = "ESS_";
        });

        // Configure TenantDbContext
        services.AddDbContext<TenantDbContext>((sp, options) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("TenantTemplateConnection");
            options.UseNpgsql(connectionString);
        });

        // Get infrastructure configuration
        var infrastructureConfig = InfrastructureConfiguration.GetInfrastructureConfiguration();

        // Add AWS configuration
        services.Configure<AwsS3Settings>(configuration.GetSection("AWS:S3"));

        // Configure AWS credentials
        var awsOptions = new AWSOptions
        {
            Credentials = new BasicAWSCredentials(
                configuration["AWS:S3:AccessKey"],
                configuration["AWS:S3:SecretKey"]),
            Region = GetValidRegionEndpoint(configuration)
        };

        // Register AWS services
        services.AddAWSService<IAmazonS3>(awsOptions);

        // Add validation for file uploads
        services.Configure<MediaValidationSettings>(options =>
        {
            options.MaxFileSize = infrastructureConfig.GetValue<long>("AWS:S3:UploadMaxSize");
            options.AllowedFileTypes = infrastructureConfig
                .GetValue<string>("AWS:S3:AllowedFileTypes")?
                .Split(',')
                .Select(x => x.Trim())
                .ToArray() ?? Array.Empty<string>();
        });

        // Register Domain Event Dispatcher
        services.AddScoped<DomainEventDispatcher>();

        // Register Services
        services.AddScoped<IDbInitializer, DbInitializer>();
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ITenantResolver, CachingTenantResolver>();
        services.AddScoped<ITenantDatabaseService, TenantDatabaseService>();
        services.AddScoped<ITenantMigrationService, TenantMigrationService>();
        services.AddScoped<TenantMigrationTracker>();
        services.AddScoped<DatabaseMigrationService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IMediaStorageService, S3MediaStorageService>();

        return services;
    }

    // Helper method to get a valid region endpoint
    private static RegionEndpoint GetValidRegionEndpoint(IConfiguration configuration)
    {
        // Try to get the region from configuration, default to us-east-1 if not specified
        string regionName = configuration["AWS:S3:Region"]?.Trim() ?? "us-east-1";

        try
        {
            // Attempt to get the region endpoint
            return RegionEndpoint.GetBySystemName(regionName);
        }
        catch (ArgumentException)
        {
            // Fallback to us-east-1 if an invalid region is provided
            Console.WriteLine($"Invalid AWS region: {regionName}. Defaulting to us-east-1.");
            return RegionEndpoint.USEast1;
        }
    }
}