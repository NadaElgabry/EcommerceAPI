using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore.Storage;

namespace EcommerceAPI.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _currentTransaction;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No active transaction to commit. Call BeginTransactionAsync first.");
            }

            try
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
        
        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                return; // nothing to roll back — safe no-op
            }

            try
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
        {
            await BeginTransactionAsync(cancellationToken);
            try
            {
                await operation();
                await CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
