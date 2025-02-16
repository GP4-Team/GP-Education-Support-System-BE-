using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MediatR;
using ESS.Application.Common.Models;
using ESS.Application.Common.Interfaces;
using ESS.Domain.Entities;
using ESS.Domain.Enums;
using ESS.Application.Features.Tenants.Commands;

namespace ESS.Application.Features.Tenants.Handlers;

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CreateTenantCommandHandler> _logger;
    private readonly ITenantDatabaseService _tenantDatabaseService;
    private readonly ITenantMigrationService _tenantMigrationService;

    public CreateTenantCommandHandler(
        IApplicationDbContext context,
        ICacheService cacheService,
        ILogger<CreateTenantCommandHandler> logger,
        ITenantDatabaseService tenantDatabaseService,
        ITenantMigrationService tenantMigrationService)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
        _tenantDatabaseService = tenantDatabaseService;
        _tenantMigrationService = tenantMigrationService;
    }

    public async Task<Result<Guid>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate tenant
            if (await _context.Tenants.AnyAsync(t => t.Identifier == request.Identifier, cancellationToken))
            {
                return Result.Failure<Guid>($"Tenant with identifier '{request.Identifier}' already exists");
            }

            if (await _context.TenantDomains.AnyAsync(td => td.Domain == request.PrimaryDomain, cancellationToken))
            {
                return Result.Failure<Guid>($"Domain '{request.PrimaryDomain}' is already in use");
            }

            // Create tenant
            var tenant = Tenant.Create(request.Name, request.Identifier, request.UseSharedDatabase);
            tenant.UpdateConnectionString(request.ConnectionString);

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
                // Save tenant and domain
                await _context.Tenants.AddAsync(tenant, cancellationToken);
                await _context.TenantDomains.AddAsync(primaryDomain, cancellationToken);

                // Add initial settings
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

                await _context.SaveChangesAsync(cancellationToken);

                // Initialize tenant database
                if (!await _tenantMigrationService.InitializeTenantDatabaseAsync(tenant, cancellationToken))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result.Failure<Guid>("Failed to initialize tenant database");
                }

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