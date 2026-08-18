namespace EcommerceAPI.Application.Interfaces
{
    public interface IUnitOfWork
    {
        /// <summary>
        /// Begins a new transaction.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task CommitAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Rolls back the current transaction.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task RollbackAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Saves all changes made in this context to the database asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The number of state entries written to the database.</returns>
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);

    }
}
