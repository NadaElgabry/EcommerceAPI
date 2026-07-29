using EcommerceAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
