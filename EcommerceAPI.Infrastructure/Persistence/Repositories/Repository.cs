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
