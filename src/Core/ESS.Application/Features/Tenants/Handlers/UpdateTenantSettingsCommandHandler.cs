using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ESS.Application.Common.Models;
using ESS.Application.Common.Interfaces;
using ESS.Application.Features.Tenants.Commands;
using ESS.Domain.Entities;

namespace ESS.Application.Features.Tenants.Handlers;

public class UpdateTenantSettingsCommandHandler : IRequestHandler<UpdateTenantSettingsCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UpdateTenantSettingsCommandHandler> _logger;

    public UpdateTenantSettingsCommandHandler(
        IApplicationDbContext context,
        ICacheService cacheService,
        ILogger<UpdateTenantSettingsCommandHandler> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(UpdateTenantSettingsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            using var transaction = await _context.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (var setting in request.Settings)
                {
                    var existingSetting = await _context.TenantSettings
                        .FirstOrDefaultAsync(ts => ts.TenantId == request.TenantId && ts.Key == setting.Key, cancellationToken);

                    if (existingSetting != null)
                    {
                        existingSetting.Value = setting.Value;
                    }
                    else
                    {
                        await _context.TenantSettings.AddAsync(new TenantSettings
                        {
                            Id = Guid.NewGuid(),
                            TenantId = request.TenantId,
                            Key = setting.Key,
                            Value = setting.Value
                        }, cancellationToken);
                    }
                }

                // Add audit log
                var auditLog = new TenantAuditLog
                {
                    Id = Guid.NewGuid(),
                    TenantId = request.TenantId,
                    Action = "SettingsUpdated",
                    Details = $"Updated {request.Settings.Count} settings",
                    Timestamp = DateTime.UtcNow
                };

                await _context.TenantAuditLogs.AddAsync(auditLog, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // Invalidate settings cache
                await _cacheService.RemoveAsync($"tenant_settings_{request.TenantId}");

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
            _logger.LogError(ex, "Error updating settings for tenant {TenantId}", request.TenantId);
            return Result.Failure<Unit>("Error updating tenant settings");
        }
    }
}