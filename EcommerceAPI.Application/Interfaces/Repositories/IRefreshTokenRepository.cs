using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<List<RefreshToken>> GetActiveByUserIdAsync(
            int userId,
            CancellationToken cancellationToken = default
        );
    }
}