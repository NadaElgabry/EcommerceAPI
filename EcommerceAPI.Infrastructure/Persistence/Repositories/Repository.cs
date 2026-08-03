using System.Linq.Expressions;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Infrastructure.Presistence.Repositories
{
    public class Repository<T> : IRepository<T>
        where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(
                new object[] { id },
                cancellationToken
            );
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

        public async Task<T?> GetByAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(
                predicate,
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
    }
}