using ESS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ESS.Infrastructure.Persistence.Configurations;
using ESS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace ESS.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Tenant> Tenants => Set<Tenant>();
    public virtual DbSet<TenantDomain> TenantDomains => Set<TenantDomain>();
    public virtual DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();
    public virtual DbSet<TenantAuditLog> TenantAuditLogs => Set<TenantAuditLog>();

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return await Database.BeginTransactionAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TenantEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TenantDomainConfiguration());
        modelBuilder.ApplyConfiguration(new TenantSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new TenantAuditLogConfiguration());

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}