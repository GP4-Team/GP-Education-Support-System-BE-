using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ESS.Application.Common.Interfaces;
using ESS.Application.Common.Models;
using ESS.Application.Features.Tenants.Commands;

namespace ESS.Application.Features.Tenants.Handlers;

public class UpdateTenantConnectionStringCommandHandler : IRequestHandler<UpdateTenantConnectionStringCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantDatabaseService _tenantDbService;
    private readonly ILogger<UpdateTenantConnectionStringCommandHandler> _logger;

    public UpdateTenantConnectionStringCommandHandler(
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ITenantDatabaseService tenantDbService,
        ILogger<UpdateTenantConnectionStringCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _tenantDbService = tenantDbService;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateTenantConnectionStringCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate the tenant exists
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken);

            if (tenant == null)
            {
                return Result.Failure($"Tenant with ID {request.TenantId} not found");
            }

            // Validate the connection string
            var isValid = await _tenantDbService.ValidateDatabaseConnectionAsync(request.ConnectionString);
            if (!isValid)
            {
                return Result.Failure("Invalid connection string or database not accessible");
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Update the connection string
                tenant.UpdateConnectionString(request.ConnectionString);

                await _unitOfWork.CommitAsync(cancellationToken);

                _logger.LogInformation("Updated connection string for tenant {TenantId}", request.TenantId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error updating connection string for tenant {TenantId}", request.TenantId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating connection string for tenant {TenantId}", request.TenantId);
            return Result.Failure($"Error updating connection string: {ex.Message}");
        }
    }
}