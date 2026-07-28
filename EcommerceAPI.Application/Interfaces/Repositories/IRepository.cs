namespace EcommerceAPI.Application.Interfaces.Repositories
{
    public interface IRepository<T> where T : class
    {
        public Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        public Task AddAsync(T entity, CancellationToken cancellationToken = default);
    }
}
