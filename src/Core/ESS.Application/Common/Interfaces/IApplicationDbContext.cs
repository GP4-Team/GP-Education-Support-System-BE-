using ESS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace ESS.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<TenantDomain> TenantDomains { get; }
    DbSet<TenantSettings> TenantSettings { get; }
    DbSet<TenantAuditLog> TenantAuditLogs { get; }

    DatabaseFacade Database { get; }
    ChangeTracker ChangeTracker { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}