using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ESS.Application.Common.Models;
using ESS.Application.Common.Interfaces;
using ESS.Application.Features.Tenants.Commands;
using ESS.Domain.Entities;

namespace ESS.Application.Features.Tenants.Handlers;

public class AddTenantDomainCommandHandler : IRequestHandler<AddTenantDomainCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<AddTenantDomainCommandHandler> _logger;

    public AddTenantDomainCommandHandler(
        IApplicationDbContext context,
        ICacheService cacheService,
        ILogger<AddTenantDomainCommandHandler> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(AddTenantDomainCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if domain already exists
            if (await _context.TenantDomains.AnyAsync(td => td.Domain == request.Domain, cancellationToken))
            {
                return Result.Failure<Guid>($"Domain '{request.Domain}' is already in use");
            }

            using var transaction = await _context.BeginTransactionAsync(cancellationToken);

            try
            {
                // If this is set as primary, unset any existing primary domain
                if (request.IsPrimary)
                {
                    var existingPrimaryDomain = await _context.TenantDomains
                        .FirstOrDefaultAsync(td => td.TenantId == request.TenantId && td.IsPrimary, cancellationToken);

                    if (existingPrimaryDomain != null)
                    {
                        existingPrimaryDomain.IsPrimary = false;
                    }
                }

                var domain = new TenantDomain
                {
                    Id = Guid.NewGuid(),
                    TenantId = request.TenantId,
                    Domain = request.Domain,
                    IsPrimary = request.IsPrimary,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.TenantDomains.AddAsync(domain, cancellationToken);

                // Add audit log
                var auditLog = new TenantAuditLog
                {
                    Id = Guid.NewGuid(),
                    TenantId = request.TenantId,
                    Action = "DomainAdded",
                    Details = $"Added domain '{request.Domain}' (Primary: {request.IsPrimary})",
                    Timestamp = DateTime.UtcNow
                };

                await _context.TenantAuditLogs.AddAsync(auditLog, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // Invalidate cache
                await _cacheService.RemoveAsync($"tenant_id_{request.TenantId}");
                await _cacheService.RemoveAsync($"tenant_domain_{request.Domain}");

                return Result.Success(domain.Id);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding domain for tenant {TenantId}", request.TenantId);
            return Result.Failure<Guid>("Error adding domain");
        }
    }
}