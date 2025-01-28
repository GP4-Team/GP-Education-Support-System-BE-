using ESS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ESS.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantSettings> TenantSettings { get; set; }
    public DbSet<TenantAuditLog> TenantAuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Identifier).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Domain).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Identifier).IsUnique();
            entity.HasIndex(e => e.Domain).IsUnique();
        });

        modelBuilder.Entity<TenantSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Value).IsRequired();
            entity.HasOne(e => e.Tenant)
                 .WithMany()
                 .HasForeignKey(e => e.TenantId)
                 .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TenantAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Details).IsRequired();
            entity.HasOne(e => e.Tenant)
                 .WithMany()
                 .HasForeignKey(e => e.TenantId)
                 .OnDelete(DeleteBehavior.Cascade);
        });
    }
}