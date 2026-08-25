using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Favorites;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface IFavoritesService
    {
        public Task AddFavoriteProductAsync(string productSlug, CancellationToken cancellationToken);
        public Task RemoveFavoriteProductAsync(string productSlug, CancellationToken cancellationToken);
        public Task AddFavoriteCategoryAsync(string slug, CancellationToken cancellationToken);
        public Task RemoveFavoriteCategoryAsync(string slug, CancellationToken cancellationToken);
        public Task<List<FavoriteCategoryResponse>> GetFavoriteCategoriesAsync(CancellationToken cancellationToken);
        public Task<CursorPagedResult<FavoriteProductResponse>> GetFavoriteProductsAsync(
            string? cursor, int pageSize, CancellationToken cancellationToken);
    }
}
