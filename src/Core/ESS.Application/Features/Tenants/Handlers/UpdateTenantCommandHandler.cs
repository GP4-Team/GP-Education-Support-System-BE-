using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ESS.Application.Common.Models;
using ESS.Application.Common.Interfaces;
using ESS.Application.Features.Tenants.Commands;

namespace ESS.Application.Features.Tenants.Handlers;

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UpdateTenantCommandHandler> _logger;

    public UpdateTenantCommandHandler(
        IApplicationDbContext context,
        ICacheService cacheService,
        ILogger<UpdateTenantCommandHandler> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (tenant == null)
            {
                return Result.Failure<Unit>($"Tenant with ID {request.Id} not found");
            }

            // Update tenant details using the proper methods
            tenant.UpdateDetails(request.Name);
            tenant.UpdateStatus(request.IsActive);

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            await _cacheService.RemoveAsync($"tenant_id_{tenant.Id}");
            await _cacheService.RemoveAsync($"tenant_identifier_{tenant.Identifier}");

            return Result.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant {TenantId}", request.Id);
            return Result.Failure<Unit>("Error updating tenant");
        }
    }
}