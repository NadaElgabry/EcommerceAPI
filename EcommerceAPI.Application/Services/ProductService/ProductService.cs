using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Product;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Image;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;
using System.Linq.Expressions;

namespace EcommerceAPI.Application.Services.ProductService
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IProductMapper _productMapper;
        private readonly IImageService _imageService;
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(
            IRepository<Product> productRepository,
            IRepository<Category> categoryRepository,
            IProductMapper productMapper,
            IImageService imageService,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _productMapper = productMapper;
            _imageService = imageService;
            _unitOfWork = unitOfWork;
        }
        public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByAsync(c => c.Id == request.CategoryId, cancellationToken);
            if (category == null)
            {
                throw new NotFoundException($"Category with ID {request.CategoryId} not found.");
            }

            var slug = request.Name.ToLowerInvariant().Replace(" ", "-");
            var existingProduct = await _productRepository.GetByAsync(p => p.Slug == slug, cancellationToken);
            if (existingProduct != null)
            {
                throw new ConflictException("A product with a similar name already exists.");
            }

            string? imageUrl = null;
            if (request.Image != null)
            {
                imageUrl = await _imageService.SaveFileAsync(request.Image, cancellationToken);
            }

            var newProduct = _productMapper.ToProduct(request, slug, imageUrl);

            if (request.TagIds != null && request.TagIds.Any())
            {
                foreach (var tagId in request.TagIds)
                {
                    newProduct.ProductTags.Add(new ProductTag
                    {
                        TagId = tagId
                    });
                }
            }

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _productRepository.AddAsync(newProduct, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);


            return _productMapper.ToProductResponse(newProduct);

        }

        public async Task<CursorPagedResponse<ProductResponse>> GetProductsPagedAsync(
     string? cursor,
     int pageSize,
     CancellationToken cancellationToken)
        {
            if (pageSize <= 0) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            var products = await _productRepository.GetPagedDescendingAsync(
                predicate: string.IsNullOrWhiteSpace(cursor)
                    ? p => true
                    : p => p.Id < CursorHelper.Decode<int>(cursor),
                orderBy: p => p.Id,
                take: pageSize + 1,
                cancellationToken: cancellationToken);

            bool hasNextPage = products.Count > pageSize;
            if (hasNextPage) products = products.Take(pageSize).ToList();

            string? nextCursor = hasNextPage && products.Count > 0
                ? CursorHelper.Encode(products[^1].Id)
                : null;

            return new CursorPagedResponse<ProductResponse>
            {
                Data = products.Select(_productMapper.ToProductResponse).ToList(),
                NextCursor = nextCursor,
                HasNext = hasNextPage
            };
        }
    }
}
