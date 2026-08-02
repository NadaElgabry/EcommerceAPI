using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Infrastructure.Contexts;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
