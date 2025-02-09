using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ESS.Application.Common.Models;
using ESS.Application.Common.Interfaces;
using ESS.Application.Features.Tenants.Commands;
using ESS.Domain.Entities;

namespace ESS.Application.Features.Tenants.Handlers;

public class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DeleteTenantCommandHandler> _logger;

    public DeleteTenantCommandHandler(
        IApplicationDbContext context,
        ICacheService cacheService,
        ILogger<DeleteTenantCommandHandler> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await _context.Tenants
                .Include(t => t.Domains)
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (tenant == null)
            {
                return Result.Failure<Unit>($"Tenant with ID '{request.Id}' not found");
            }

            using var transaction = await _context.BeginTransactionAsync(cancellationToken);

            try
            {
                // Log the deletion
                var auditLog = new TenantAuditLog
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.Id,
                    Action = "Deleted",
                    Details = $"Tenant '{tenant.Name}' ({tenant.Identifier}) deleted",
                    Timestamp = DateTime.UtcNow
                };

                await _context.TenantAuditLogs.AddAsync(auditLog, cancellationToken);

                // Remove tenant domains
                foreach (var domain in tenant.Domains)
                {
                    _context.TenantDomains.Remove(domain);
                }

                // Remove tenant
                _context.Tenants.Remove(tenant);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // Clear cache
                await _cacheService.RemoveAsync($"tenant_id_{tenant.Id}");
                await _cacheService.RemoveAsync($"tenant_identifier_{tenant.Identifier}");
                foreach (var domain in tenant.Domains)
                {
                    await _cacheService.RemoveAsync($"tenant_domain_{domain.Domain}");
                }

                return Result.Success(Unit.Value);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tenant {TenantId}", request.Id);
            return Result.Failure<Unit>("Error deleting tenant");
        }
    }
}