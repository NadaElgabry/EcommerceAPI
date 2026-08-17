using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EcommerceAPI.Infrastructure.Persistence.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _dbSet = context.Set<T>();
        }

        public async Task AddAsync(
            T entity,
            CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(
                entity,
                cancellationToken
            );
        }

        public async Task<bool> ExistByAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(
                predicate,
                cancellationToken
            );
        }

        public async Task<T?> GetByAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(
                predicate,
                cancellationToken
            );
        }

        public async Task<List<T>> GetAllByAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
    }
}