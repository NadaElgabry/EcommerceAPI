using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Infrastructure.Contexts;

namespace EcommerceAPI.Infrastructure.Presistence.Repositories
{
    public class RoleRepository
        : Repository<Role>, IRoleRepository
    {
        public RoleRepository(AppDbContext context)
            : base(context)
        {
        }
    }
}