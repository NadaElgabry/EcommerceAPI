using System.Linq.Expressions;

namespace EcommerceAPI.Application.Interfaces.Repositories
{
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets all entities of type T.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task AddAsync(T entity,CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets all entities of type T.
        /// </summary>
        /// <param name="predicate">The expression to filter entities.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Entity if exists</returns>
        public Task<T?> GetByAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        public Task<bool> ExistByAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        public void Update(T entity);
        public void Delete(T entity);
    }
}
