//using ESS.Domain.Entities;
//using Microsoft.EntityFrameworkCore;
//using ESS.Infrastructure.Persistence.Configurations;
//using ESS.Application.Common.Interfaces;
//using Microsoft.EntityFrameworkCore.Storage;
//using ESS.Domain.Entities.Media;


//namespace ESS.Infrastructure.Persistence;

//public class ApplicationDbContext : DbContext, IApplicationDbContext
//{
//    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//        : base(options)
//    {
//    }

//    public virtual DbSet<Tenant> Tenants => Set<Tenant>();
//    public virtual DbSet<TenantDomain> TenantDomains => Set<TenantDomain>();
//    public virtual DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();
//    public virtual DbSet<TenantAuditLog> TenantAuditLogs => Set<TenantAuditLog>();
//    public virtual DbSet<Domain.Entities.Media.Media> Media => Set<Domain.Entities.Media.Media>();
//    public virtual DbSet<MediaCollection> MediaCollections => Set<MediaCollection>();

//    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
//    {
//        return await Database.BeginTransactionAsync(cancellationToken);
//    }

//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        modelBuilder.ApplyConfiguration(new TenantEntityConfiguration());
//        modelBuilder.ApplyConfiguration(new TenantDomainConfiguration());
//        modelBuilder.ApplyConfiguration(new TenantSettingsConfiguration());
//        modelBuilder.ApplyConfiguration(new TenantAuditLogConfiguration());
//    }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//    {
//        optionsBuilder.ConfigureWarnings(warnings =>
//            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

//        base.OnConfiguring(optionsBuilder);
//    }
//}


using ESS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ESS.Infrastructure.Persistence.Configurations;
using ESS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using ESS.Domain.Entities.Media;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

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

    // Explicitly specify the Media type from the namespace
    public virtual DbSet<ESS.Domain.Entities.Media.Media> Media => Set<ESS.Domain.Entities.Media.Media>();
    public virtual DbSet<MediaCollection> MediaCollections => Set<MediaCollection>();

    // Override base members to implement interface
    public override DatabaseFacade Database => base.Database;
    public override ChangeTracker ChangeTracker => base.ChangeTracker;

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return await Database.BeginTransactionAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Existing configurations
        modelBuilder.ApplyConfiguration(new TenantEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TenantDomainConfiguration());
        modelBuilder.ApplyConfiguration(new TenantSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new TenantAuditLogConfiguration());

        // Configure Media entity
        modelBuilder.Entity<ESS.Domain.Entities.Media.Media>(entity =>
        {
            // Configure owned MediaFile
            entity.OwnsOne(m => m.File, fb =>
            {
                fb.Property(f => f.FileName).HasColumnName("FileName");
                fb.Property(f => f.FileType).HasColumnName("FileType");
                fb.Property(f => f.MimeType).HasColumnName("MimeType");
                fb.Property(f => f.Size).HasColumnName("FileSize");
            });

            // Configure relationships
            entity.HasOne(m => m.MediaCollection)
                  .WithMany(mc => mc.Media)
                  .HasForeignKey(m => m.MediaCollectionId)
                  .IsRequired();

            // Configure indexes and constraints
            entity.HasIndex(m => new { m.TenantId, m.ResourceId, m.ResourceType });
            entity.Property(m => m.FilePath).HasMaxLength(500);
        });

        // Configure MediaCollection entity
        modelBuilder.Entity<MediaCollection>(entity =>
        {
            entity.HasIndex(mc => new { mc.TenantId, mc.Name }).IsUnique();

            entity.Property(mc => mc.Name).HasMaxLength(100).IsRequired();
            entity.Property(mc => mc.AllowedTypes).HasMaxLength(200);

            // Configure relationships
            entity.HasMany(mc => mc.Media)
                  .WithOne(m => m.MediaCollection)
                  .HasForeignKey(m => m.MediaCollectionId);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

        base.OnConfiguring(optionsBuilder);
    }
}