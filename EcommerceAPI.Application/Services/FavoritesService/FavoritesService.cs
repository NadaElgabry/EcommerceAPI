using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Favorites;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.DTOs.Favorites;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Application.Services.FavoritesService
{
    public class FavoritesService : IFavoritesService
    {
        private readonly IRepository<FavoriteProduct> _favoriteProductRepository;
        private readonly IRepository<FavoriteCategory> _favoriteCategoryRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IUserActivityService _userActivityService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public FavoritesService(
            IRepository<FavoriteProduct> favoriteProductRepository,
            IRepository<FavoriteCategory> favoriteCategoryRepository,
            IRepository<User> userRepository,
            IRepository<Product> productRepository,
            IRepository<Category> categoryRepository,
            IUserActivityService userActivityService,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _favoriteProductRepository = favoriteProductRepository;
            _favoriteCategoryRepository = favoriteCategoryRepository;
            _userRepository = userRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _userActivityService = userActivityService;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task AddFavoriteProductAsync(string productSlug, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByAsync(p => p.Slug == productSlug, cancellationToken)
                ?? throw new NotFoundException($"Product '{productSlug}' not found.");

            var user = await _userRepository.GetByAsync(u => u.Guid == _currentUserService.UserGuid, cancellationToken)
                ?? throw new NotFoundException("User not found.");

            if (await _favoriteProductRepository.ExistByAsync(
                f => f.UserId == user.Id && f.ProductId == product.Id, cancellationToken))
            {
                throw new ConflictException("Product is already in favorites.");
            }

            var favorite = new FavoriteProduct { UserId = user.Id, ProductId = product.Id };

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _favoriteProductRepository.AddAsync(favorite, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);

            await _userActivityService.LogActivityAsync(
                user.Id, product.Id, UserActionType.AddedToFavorites, cancellationToken);
        }

        public async Task RemoveFavoriteProductAsync(string productSlug, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByAsync(p => p.Slug == productSlug, cancellationToken)
                ?? throw new NotFoundException($"Product '{productSlug}' not found.");
            var user = await _userRepository.GetByAsync(u => u.Guid == _currentUserService.UserGuid, cancellationToken)
                 ?? throw new NotFoundException("User not found.");

            var favorite = await _favoriteProductRepository.GetByAsync(
                f => f.UserId == user.Id && f.ProductId == product.Id, cancellationToken)
                ?? throw new NotFoundException("Product is not in favorites.");

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                _favoriteProductRepository.Delete(favorite);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);

            await _userActivityService.LogActivityAsync(
                user.Id, product.Id, UserActionType.RemovedFromFavorites, cancellationToken);
        }

        public async Task AddFavoriteCategoryAsync(string slug, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByAsync(c => c.Slug == slug, cancellationToken)
                ?? throw new NotFoundException($"Category with slug '{slug}' not found.");

            var user = await _userRepository.GetByAsync(u => u.Guid == _currentUserService.UserGuid, cancellationToken)
                ?? throw new NotFoundException("User not found.");

            if (await _favoriteCategoryRepository.ExistByAsync(
                f => f.UserId == user.Id && f.CategoryId == category.Id, cancellationToken))
            {
                throw new ConflictException("Category is already in favorites.");
            }

            var favorite = new FavoriteCategory { UserId = user.Id, CategoryId = category.Id };

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _favoriteCategoryRepository.AddAsync(favorite, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
        }

        public async Task RemoveFavoriteCategoryAsync(string slug, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByAsync(u => u.Guid == _currentUserService.UserGuid, cancellationToken)
                ?? throw new NotFoundException("User not found.");

            var category = await _categoryRepository.GetByAsync(c => c.Slug == slug, cancellationToken)
                ?? throw new NotFoundException($"Category with slug '{slug}' not found.");

            var favorite = await _favoriteCategoryRepository.GetByAsync(
                f => f.UserId == user.Id && f.CategoryId == category.Id, cancellationToken)
                ?? throw new NotFoundException("Category is not in favorites.");

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                _favoriteCategoryRepository.Delete(favorite);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
        }

        public async Task<CursorPagedResult<FavoriteProductResponse>> GetFavoriteProductsAsync(
            string? cursor, int pageSize, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByAsync(u => u.Guid == _currentUserService.UserGuid, cancellationToken)
                ?? throw new NotFoundException("User not found.");

            if (pageSize <= 0 || pageSize > 50) pageSize = 20;

            var favorites = await _favoriteProductRepository.GetPagedAsync(
                predicate: string.IsNullOrWhiteSpace(cursor)
                    ? f => f.UserId == user.Id
                    : f => f.UserId == user.Id && f.Id < CursorHelper.Decode<int>(cursor),
                orderBy: f => f.Id,
                include: query => query.Include(f => f.Product),
                take: pageSize + 1,
                cancellationToken: cancellationToken);

            bool hasNext = favorites.Count > pageSize;
            if (hasNext) favorites = favorites.Take(pageSize).ToList();

            return new CursorPagedResult<FavoriteProductResponse>
            {
                Data = favorites.Select(f => new FavoriteProductResponse
                {
                    ProductId = f.ProductId,
                    Slug = f.Product.Slug,
                    Name = f.Product.Name,
                    Price = f.Product.Price,
                    ProductImage = f.Product.ProductImage,
                    AddedAt = f.AddedAt
                }).ToList(),
                Pagination = new CursorPageInfo
                {
                    NextCursor = hasNext ? CursorHelper.Encode(favorites[^1].Id) : null,
                    HasNext = hasNext,
                    PageSize = pageSize
                }
            };
        }

        public async Task<List<FavoriteCategoryResponse>> GetFavoriteCategoriesAsync(CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByAsync(u => u.Guid == _currentUserService.UserGuid, cancellationToken)
                ?? throw new NotFoundException("User not found.");

            var favorites = await _favoriteCategoryRepository.GetAllByAsync(
                f => f.UserId == user.Id,
                cancellationToken,
                include: query => query.Include(f => f.Category));

            return favorites.Select(f => new FavoriteCategoryResponse
            {
                CategoryId = f.CategoryId,
                Name = f.Category.Name,
                slug = f.Category.Slug,
                AddedAt = f.AddedAt
            }).ToList();
        }
    }
}