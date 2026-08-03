using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Infrastructure.Contexts;

namespace EcommerceAPI.Infrastructure.Presistence.Repositories
{
    public class UserRepository
        : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context)
            : base(context)
        {
        }
    }
}