using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using ESS.Domain.Interfaces;
using ESS.Infrastructure.MultiTenancy;
using Finbuckle.MultiTenant.Abstractions;

namespace ESS.Infrastructure.Persistence;

public class TenantDbContext : DbContext
{
    private readonly IMultiTenantContext<EssTenantInfo>? _tenantContext;
    private readonly IConfiguration _configuration;

    // Add DbSet properties for tenant-specific entities

    public TenantDbContext(
        DbContextOptions<TenantDbContext> options,
        IMultiTenantContextAccessor<EssTenantInfo> tenantContextAccessor,
        IConfiguration configuration)
        : base(options)
    {
        _tenantContext = tenantContextAccessor?.MultiTenantContext;
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = _tenantContext?.TenantInfo?.ConnectionString
                ?? _configuration.GetConnectionString("TenantTemplateConnection");

            optionsBuilder.UseNpgsql(connectionString,
                npgsqlOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                });
        }

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        // Apply tenant filter to all entity types that implement ITenantEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(ITenantEntity).IsAssignableFrom(e.ClrType)))
        {
            var parameter = Expression.Parameter(entityType.ClrType, "entity");
            var property = Expression.Property(parameter, "TenantId");
            var tenantId = Expression.Constant(_tenantContext?.TenantInfo?.Id);
            var filter = Expression.Lambda(Expression.Equal(property, tenantId), parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}