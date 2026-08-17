using System.Linq.Expressions;

namespace EcommerceAPI.Application.Interfaces.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task AddAsync(
            T entity,
            CancellationToken cancellationToken = default
        );

        Task<T?> GetByAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default
        );

        Task<List<T>> GetAllByAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default
        );

        Task<bool> ExistByAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default
        );

        void Update(T entity);

        void Delete(T entity);
    }
}