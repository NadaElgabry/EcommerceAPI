using System.Linq.Expressions;

namespace EcommerceAPI.Application.Interfaces.Repositories
{
    public interface IRepository<T> where T : class
    {
        public Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        public Task AddAsync(T entity, CancellationToken cancellationToken = default);
        public Task<T?> GetByAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        public Task<bool> ExistByAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
