using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

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

            if (include != null)
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
        public async Task<List<T>> GetAllAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            IQueryable<T> query = _dbSet.Where(predicate);

            if (include != null)
            {
                query = include(query);
            }

            return await query.ToListAsync(cancellationToken);
        }
        public async Task<List<T>> GetPagedDescendingAsync<TKey>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TKey>> orderBy,
            int take,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet.Where(predicate);
            if (include != null)
            {
                query = include(query);
            }

            return await query
                .OrderByDescending(orderBy)
                .Take(take)
                .ToListAsync(cancellationToken);
        }
        public async Task<List<T>> GetAllAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.ToListAsync(cancellationToken);
        }
        public async Task<List<T>> GetPagedAsync<TKey>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TKey>> orderBy,
            int take,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _context.Set<T>();

            if (include != null)
            {
                query = include(query);
            }

            return await query
                .Where(predicate)
                .OrderBy(orderBy)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<T>> GetPageOffSetAsync<TKey>(
        Expression<Func<T, TKey>> orderBy,
        int take,
        int skip,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include=null,
        CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _context.Set<T>();

            if (include != null)
            {
                query = include(query);
            }

            return await query
                .OrderBy(orderBy)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }
        public async Task<long> GetCountAsync(CancellationToken cancellationToken)
        {
            return await _dbSet.LongCountAsync(cancellationToken);
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