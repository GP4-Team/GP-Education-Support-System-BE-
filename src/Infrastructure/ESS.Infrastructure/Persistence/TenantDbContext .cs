using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using ESS.Domain.Interfaces;
using ESS.Infrastructure.MultiTenancy;
using Finbuckle.MultiTenant.Abstractions;
using ESS.Domain.Entities.Users;
using ESS.Infrastructure.Persistence.Configurations.Users;

namespace ESS.Infrastructure.Persistence;

public class TenantDbContext : DbContext
{
    private readonly IMultiTenantContext<EssTenantInfo>? _tenantContext;
    private readonly IConfiguration _configuration;

    // Add DbSet properties for tenant-specific entities
    public virtual DbSet<User> Users => Set<User>();

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

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("No valid connection string found for tenant database.");
            }

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
            var property = Expression.Property(parameter, nameof(ITenantEntity.TenantId));

            // Get the tenant identifier with null check
            var currentTenantId = _tenantContext?.TenantInfo?.Identifier ?? string.Empty;
            var tenantId = Expression.Constant(currentTenantId, typeof(string));

            var filter = Expression.Lambda(Expression.Equal(property, tenantId), parameter);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }

        base.OnModelCreating(modelBuilder);
    }
}