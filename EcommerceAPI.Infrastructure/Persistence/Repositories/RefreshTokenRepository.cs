using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public RefreshTokenRepository(AppDbContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<List<RefreshToken>> GetActiveByUserIdAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<RefreshToken>()
                .Where(token =>
                    token.UserId == userId &&
                    token.RevokedAt == null &&
                    token.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellationToken);
        }
    }
}