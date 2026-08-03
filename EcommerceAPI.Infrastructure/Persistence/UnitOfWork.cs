using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Infrastructure.Contexts;

namespace EcommerceAPI.Infrastructure.Presistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(
                cancellationToken
            );
        }
    }
}