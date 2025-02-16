using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using ESS.Infrastructure.MultiTenancy;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;

namespace ESS.Infrastructure.Persistence;

public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        // Navigate up to the API project directory
        var apiProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Presentation", "ESS.API");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .Build();

        var builder = new DbContextOptionsBuilder<TenantDbContext>();
        var connectionString = configuration.GetConnectionString("TenantTemplateConnection");
        builder.UseNpgsql(connectionString);

        // Create a mock tenant context accessor for design-time
        var mockTenantContextAccessor = new MockTenantContextAccessor();

        return new TenantDbContext(builder.Options, mockTenantContextAccessor, configuration);
    }
}

// Mock class for design-time migrations
public class MockTenantContextAccessor : IMultiTenantContextAccessor<EssTenantInfo>, IMultiTenantContextAccessor
{
    IMultiTenantContext<EssTenantInfo>? IMultiTenantContextAccessor<EssTenantInfo>.MultiTenantContext => null;
    IMultiTenantContext? IMultiTenantContextAccessor.MultiTenantContext => null;
}