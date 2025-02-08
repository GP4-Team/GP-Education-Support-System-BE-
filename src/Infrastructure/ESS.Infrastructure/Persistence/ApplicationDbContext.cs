using ESS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ESS.Infrastructure.Persistence.Configurations;

namespace ESS.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantDomain> TenantDomains { get; set; }
    public DbSet<TenantSettings> TenantSettings { get; set; }
    public DbSet<TenantAuditLog> TenantAuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TenantEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TenantDomainConfiguration());
        modelBuilder.ApplyConfiguration(new TenantSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new TenantAuditLogConfiguration());

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}