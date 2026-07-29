using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces
{
    public interface IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
