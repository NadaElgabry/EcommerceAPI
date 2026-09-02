using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Product;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Image;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Interfaces.Search;
using EcommerceAPI.Application.Interfaces.Slug;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace EcommerceAPI.Application.Services.ProductService
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Tag> _tagRepository;
        private readonly IProductMapper _productMapper;
        private readonly IImageService _imageService;
        private readonly IUserActivityService _userActivityService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISlugGenerator _slugGenerator;

        private readonly IProductSearchService _searchService;
        private readonly IProductIndexingService _indexingService;

        private readonly ILogger<ProductService> _logger;
        public ProductService(
            IRepository<Product> productRepository,
            IRepository<Category> categoryRepository,
            IRepository<User> userRepository,
            IRepository<Tag> tagRepository,
            IProductMapper productMapper,
            IImageService imageService,
            IUserActivityService userActivityService,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            ISlugGenerator slugGenerator,
            IProductSearchService searchService,
            IProductIndexingService indexingService,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _tagRepository = tagRepository;
            _productMapper = productMapper;
            _imageService = imageService;
            _userActivityService = userActivityService;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _slugGenerator = slugGenerator;
            _searchService = searchService;
            _indexingService = indexingService;
            _logger = logger ;
        }

        public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken)
        {

            var category = await _categoryRepository.GetByAsync(c => c.Slug == request.CategorySlug, cancellationToken);
            if (category == null)
            {
                throw new NotFoundException($"Category with slug {request.CategorySlug} not found.");
            }

            // 2. Generate and validate Slug
            var slug = _slugGenerator.GenerateSlug(request.Name);
            if (await _productRepository.ExistByAsync(p => p.Slug == slug,cancellationToken))
            {
                throw new ConflictException("A product with a similar name already exists.");
            }

            string? imageUrl = null;
            if (request.Image != null)
            {
                // SaveFileAsync takes an IFormFile and CancellationToken
                imageUrl = await _imageService.SaveFileAsync(request.Image,slug,ImageOwnerType.Product, cancellationToken);
            }

            var validTags = new List<Tag>();
            if (request.TagNames != null && request.TagNames.Any())
            {
                foreach (var tagName in request.TagNames.Distinct())
                {
                    var tag = await _tagRepository.GetByAsync(t => t.Name == tagName, cancellationToken);
                    if (tag != null)
                    {
                        validTags.Add(tag);
                    }
                }
            }

            var newProduct = _productMapper.ToProduct(request, slug,category, imageUrl, validTags);


            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _productRepository.AddAsync(newProduct, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
            await _indexingService.IndexProductAsync(newProduct, cancellationToken);

            return _productMapper.ToProductResponse(newProduct);

        }

        public async Task<ProductResponse> GetProductDetailsAsync(string slug, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByAsync(
                predicate: p => p.Slug == slug,
                include: query => query
                .Include(p => p.Category)
                .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag),
                cancellationToken: cancellationToken)
                ?? throw new NotFoundException($"Product '{slug}' not found.");

            if (_currentUserService.IsAuthenticated && _currentUserService.Role == "Customer")
            {
                var user = await _userRepository.GetByAsync(u => u.Guid == _currentUserService.UserGuid, cancellationToken)
                    ?? throw new NotFoundException("User not found.");

                await _userActivityService.LogActivityAsync(
                    user.Id,
                    product.Id,
                    UserActionType.ViewProduct,
                    cancellationToken
                );
            }

            return _productMapper.ToProductResponse(product);
        }

        public async Task<ProductResponse> UpdateProductAsync(
            string slug, UpdateProductRequest request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByAsync(
                predicate: p => p.Slug == slug,
                include: query => query
                .Include(p => p.Category)
                .Include(p => p.ProductTags),
                cancellationToken: cancellationToken)
                ?? throw new NotFoundException($"Product '{slug}' not found.");

            if (request.CategorySlug != product.Category.Slug)
            {
                var CategorySlug = _slugGenerator.GenerateSlug(request.CategorySlug);
                var categoryExists = await _categoryRepository.ExistByAsync(
                    c => c.Slug == CategorySlug, cancellationToken);
                if (!categoryExists)
                {
                    throw new NotFoundException($"Category with slug {request.CategorySlug} not found.");
                }
                var category = await _categoryRepository.GetByAsync(c => c.Slug == CategorySlug, cancellationToken);

                product.CategoryId = category.Id;
            }

            if (!string.Equals(request.Name, product.Name, StringComparison.Ordinal))
            {
                var newSlug = _slugGenerator.GenerateSlug(request.Name);
                var slugTaken = await _productRepository.ExistByAsync(
                    p => p.Slug == newSlug && p.Id != product.Id, cancellationToken);
                if (slugTaken)
                {
                    throw new ConflictException("A product with a similar name already exists.");
                }
                product.Slug = newSlug;
                product.Name = request.Name;
            }

            if (request.Image != null)
            { 
                product.ProductImage = await _imageService.SaveFileAsync(request.Image, product.Slug, ImageOwnerType.Product, cancellationToken);
            }

            _productMapper.UpdateProductFromRequest(product, request);

            product.ProductTags.Clear();
            foreach (var tagName in request.TagNames.Distinct())
            {
                var tag = await _tagRepository.GetByAsync(t => t.Name == tagName, cancellationToken);
                if (tag == null)
                {
                    throw new NotFoundException($"Tag with name {tagName} not found.");
                }
                product.ProductTags.Add(new ProductTag { ProductId = product.Id, TagId = tag.Id });
            }

            _productRepository.Update(product);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);

            var productForIndexing = await _productRepository.GetByAsync(
                                        p => p.Id == product.Id,
                                        include: query => query
                                            .Include(p => p.Category)
                                            .Include(p => p.ProductTags)
                                                .ThenInclude(pt => pt.Tag),
                                        cancellationToken: cancellationToken);

            try
            {
                await _indexingService.IndexProductAsync(productForIndexing!, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to index product {ProductId} after save. Product data is out of sync with search until next reindex.", product.Id);
            }

            return _productMapper.ToProductResponse(product);
        }


        ///<inheritdoc/>
        public async Task<CursorPagedResult<ProductSummaryResponse>> SearchProductsAsync(
            ProductQueryParamsRequest queryParams, CancellationToken cancellationToken)
        {
            var result = await _searchService.SearchProductsAsync(queryParams, cancellationToken);

            var userId = _currentUserService.UserGuid;

            _ = LogSearchActivitiesAsync(userId, result.Data, CancellationToken.None);

            return result;
        }

        private async Task LogSearchActivitiesAsync(Guid userId, IEnumerable<ProductSummaryResponse> products, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetByAsync(predicate: u => u.Guid == userId, cancellationToken: cancellationToken);
                

                var slugs = products.Select(p => p.Slug).ToList();
                var productEntities = await _productRepository.GetAllAsync(
                    predicate: p => slugs.Contains(p.Slug),
                    cancellationToken: cancellationToken);

                var idBySlug = productEntities.ToDictionary(p => p.Slug, p => p.Id);

                foreach (var product in products)
                {
                    if (idBySlug.TryGetValue(product.Slug, out var productId))
                    {
                        await _userActivityService.LogActivityAsync(
                            user.Id,
                            productId,
                            UserActionType.SearchProduct,
                            cancellationToken
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log search activities for user {UserId}", userId);
            }
        }

        public async Task DeleteProductAsync(string slug, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByAsync(
                p => p.Slug == slug, cancellationToken)
                ?? throw new NotFoundException($"Product '{slug}' not found.");

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                _productRepository.Delete(product);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);

            try
            {
                await _indexingService.DeleteProductAsync(product.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove product {ProductId} from search index after delete. Product data is out of sync with search until next reindex.", product.Id);
            }
        }


    }
}
