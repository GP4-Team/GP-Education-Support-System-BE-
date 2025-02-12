using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ESS.Application.Common.Models;
using ESS.Application.Common.Interfaces;
using ESS.Application.Features.Tenants.Commands;
using ESS.Domain.Entities;
using ESS.Domain.Enums;

namespace ESS.Application.Features.Tenants.Handlers;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CreateTenantCommandHandler> _logger;
    private readonly ITenantDatabaseService _tenantDatabaseService;

    public CreateTenantCommandHandler(
        IApplicationDbContext context,
        ICacheService cacheService,
        ILogger<CreateTenantCommandHandler> logger,
        ITenantDatabaseService tenantDatabaseService)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
        _tenantDatabaseService = tenantDatabaseService;
    }

    public async Task<Result<Guid>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if tenant identifier already exists
            if (await _context.Tenants.AnyAsync(t => t.Identifier == request.Identifier, cancellationToken))
            {
                return Result.Failure<Guid>($"Tenant with identifier '{request.Identifier}' already exists");
            }

            // Check if domain already exists
            if (await _context.TenantDomains.AnyAsync(td => td.Domain == request.PrimaryDomain, cancellationToken))
            {
                return Result.Failure<Guid>($"Domain '{request.PrimaryDomain}' is already in use");
            }

            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Identifier = request.Identifier,
                ConnectionString = request.ConnectionString,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                DatabaseStatus = TenantDatabaseStatus.Pending // Set initial database status
            };

            var primaryDomain = new TenantDomain
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Domain = request.PrimaryDomain,
                IsPrimary = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                await _context.Tenants.AddAsync(tenant, cancellationToken);
                await _context.TenantDomains.AddAsync(primaryDomain, cancellationToken);

                // Add initial settings if provided
                if (request.InitialSettings?.Any() == true)
                {
                    var settings = request.InitialSettings.Select(kvp => new TenantSettings
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenant.Id,
                        Key = kvp.Key,
                        Value = kvp.Value
                    });

                    await _context.TenantSettings.AddRangeAsync(settings, cancellationToken);
                }

                // Add audit log for tenant creation
                var auditLog = new TenantAuditLog
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.Id,
                    Action = "Created",
                    Details = $"Tenant created with identifier '{request.Identifier}'",
                    Timestamp = DateTime.UtcNow
                };

                await _context.TenantAuditLogs.AddAsync(auditLog, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                // Create tenant database
                var databaseCreated = await _tenantDatabaseService.CreateTenantDatabaseAsync(tenant);

                if (!databaseCreated)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result.Failure<Guid>("Failed to create tenant database");
                }

                // Add audit log for database creation
                var dbAuditLog = new TenantAuditLog
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.Id,
                    Action = "DatabaseCreated",
                    Details = $"Database created for tenant '{request.Identifier}'",
                    Timestamp = DateTime.UtcNow
                };

                await _context.TenantAuditLogs.AddAsync(dbAuditLog, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // Invalidate cache
                await _cacheService.RemoveAsync($"tenant_id_{tenant.Id}");
                await _cacheService.RemoveAsync($"tenant_identifier_{tenant.Identifier}");

                return Result.Success(tenant.Id);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant");
            return Result.Failure<Guid>("Error creating tenant");
        }
    }
}