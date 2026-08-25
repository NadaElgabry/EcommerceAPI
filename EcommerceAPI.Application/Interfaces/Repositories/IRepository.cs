using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace EcommerceAPI.Application.Interfaces.Repositories
{
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Adds a new entity to the database asynchronously.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public Task AddAsync(T entity,CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets all entities of requested type.
        /// </summary>
        /// <param name="predicate">The expression to filter entities.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Entity if exists</returns>
        public Task<T?> GetByAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
      
        public Task<T?> GetByAsync(Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IIncludableQueryable<T, Object>>? include = null,
            CancellationToken cancellationToken = default);
        /// <summary>
        /// Checks if an entity of requested type exists that matches the given predicate.
        /// </summary>
        /// <param name="predicate">The expression to filter entities.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>true if an entity exists; otherwise, false.</returns>
        public Task<bool> ExistByAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        /// <summary>
        ///  Deletes all entities of requested type that match the given predicate.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task DeleteAllByAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        /// <summary>
        /// Updates an existing entity of requested type.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// 
        Task<List<T>> GetAllAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        CancellationToken cancellationToken = default);
        public Task<List<T>> GetPagedAsync<TKey>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TKey>> orderBy,           
            int take,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            CancellationToken cancellationToken = default);
        public Task<List<T>> GetPagedDescendingAsync<TKey>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TKey>> orderBy,
            int take,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            CancellationToken cancellationToken = default);

        Task<List<T>> GetPageOffSetAsync<TKey>(
            Expression<Func<T, TKey>> orderBy,
            int take,
            int skip,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include=null,
            CancellationToken cancellationToken = default);
        public Task<long> GetCountAsync(CancellationToken cancellationToken = default);

        public void Update(T entity);
        /// <summary>
        /// Deletes an entity of requested type from the database.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        public void Delete(T entity);
    }
}