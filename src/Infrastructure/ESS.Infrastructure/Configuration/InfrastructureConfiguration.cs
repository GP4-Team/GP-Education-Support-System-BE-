// src/Infrastructure/ESS.Infrastructure/Configuration/InfrastructureConfiguration.cs
using Microsoft.Extensions.Configuration;

namespace ESS.Infrastructure.Configuration;

public static class InfrastructureConfiguration
{
    public static IConfiguration GetInfrastructureConfiguration()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddJsonFile("appsettings.Infrastructure.json", optional: true)
            .Build();
    }
}