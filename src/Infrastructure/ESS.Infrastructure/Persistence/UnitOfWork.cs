using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using ESS.Application.Common.Interfaces;
using ESS.Domain.Common;
using ESS.Infrastructure.DomainEvents;

namespace ESS.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UnitOfWork> _logger;
    private readonly DomainEventDispatcher _domainEventDispatcher;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    public UnitOfWork(
        IApplicationDbContext context,
        ICacheService cacheService,
        ILogger<UnitOfWork> logger,
        DomainEventDispatcher domainEventDispatcher)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            _logger.LogWarning("A transaction is already in progress");
            return _currentTransaction;
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        _logger.LogInformation("Transaction began with ID: {TransactionId}", _currentTransaction.TransactionId);

        return _currentTransaction;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);

            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
                _logger.LogInformation("Transaction committed with ID: {TransactionId}", _currentTransaction.TransactionId);
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during transaction commit");
            await RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            return;
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            _logger.LogInformation("Transaction rolled back with ID: {TransactionId}", _currentTransaction.TransactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during transaction rollback");
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Get entities with domain events before saving
            var entitiesWithEvents = _context.ChangeTracker.Entries<IHasDomainEvents>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToArray();

            // Save changes
            var result = await _context.SaveChangesAsync(cancellationToken);

            // Dispatch events after successful save
            if (entitiesWithEvents.Any())
            {
                await _domainEventDispatcher.DispatchEventsAsync(entitiesWithEvents, cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during SaveChanges");
            throw;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
            _disposed = true;
        }
    }
}