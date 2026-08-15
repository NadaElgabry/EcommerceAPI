using System.Linq.Expressions;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace EcommerceAPI.Infrastructure.Persistence.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;
        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }
        /// <inheritdoc />
        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<bool> ExistByAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }
        /// <inheritdoc />
        public async Task<T?> GetByAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<T?> GetByAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _context.Set<T>();

            if (include!= null)
            {
                query = include(query);
            }
            return await query.FirstOrDefaultAsync(predicate, cancellationToken);
        }
        /// <inheritdoc />
        public async Task DeleteAllByAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var entities = await _dbSet.Where(predicate).ToListAsync(cancellationToken);
            _dbSet.RemoveRange(entities);
        }

        public async Task<List<T>> GetPagedAsync<TKey1, TKey2>(
    Expression<Func<T, bool>> predicate,
    Expression<Func<T, TKey1>> orderBy,
    Expression<Func<T, TKey2>> thenBy,
    int take,
    CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>()
                .Where(predicate)
                .OrderBy(orderBy)
                .ThenBy(thenBy)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<T>> GetPagedDescendingAsync<TKey1, TKey2>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TKey1>> orderBy,
            Expression<Func<T, TKey2>> thenBy,
            int take,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>()
                .Where(predicate)
                .OrderByDescending(orderBy)
                .ThenByDescending(thenBy)
                .Take(take)
                .ToListAsync(cancellationToken);
        }


        /// <inheritdoc />
        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
        /// <inheritdoc />
        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
    }
}
