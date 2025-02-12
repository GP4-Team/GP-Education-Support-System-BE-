using Microsoft.EntityFrameworkCore.Storage;

namespace ESS.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
