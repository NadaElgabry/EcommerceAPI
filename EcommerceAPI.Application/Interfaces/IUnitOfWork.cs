using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces
{
    public interface IUnitOfWork
    {
        /// <summary>
        /// Saves all changes made in this context to the database asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The number of state entries written to the database.</returns>
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
