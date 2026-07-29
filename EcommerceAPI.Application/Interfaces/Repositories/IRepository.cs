using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace EcommerceAPI.Application.Interfaces.Repositories
{
    public interface IRepository<T> where T : class
    {
        public Task AddAsync(T entity, CancellationToken cancellationToken = default);
        public Task<T?> GetByAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        public Task<T?> GetByAsync(Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IIncludableQueryable<T, Object>>? include = null,
            CancellationToken cancellationToken = default);
        public Task<bool> ExistByAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        public void Update(T entity);

        public void Delete(T entity);
    }
}
