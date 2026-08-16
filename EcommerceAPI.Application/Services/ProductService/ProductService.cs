using EcommerceAPI.Application.DTOs.Product;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Image;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;

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
            // 1. Validate Category exists
            var category = await _categoryRepository.GetByAsync(c => c.Id == request.CategoryId, cancellationToken);
            if (category == null)
            {
                throw new NotFoundException($"Category with ID {request.CategoryId} not found.");
            }

            // 2. Generate and validate Slug
            var slug = request.Name.ToLowerInvariant().Replace(" ", "-");
            var existingProduct = await _productRepository.GetByAsync(p => p.Slug == slug, cancellationToken);
            if (existingProduct != null)
            {
                throw new ConflictException("A product with a similar name already exists.");
            }

            // 3. Handle Image Upload
            string? imageUrl = null;
            if (request.Image != null)
            {
                // SaveFileAsync takes an IFormFile and CancellationToken
                imageUrl = await _imageService.SaveFileAsync(request.Image, cancellationToken);
            }

            // 4. Map Request to Domain Entity
            var newProduct = _productMapper.ToProduct(request, slug, imageUrl);

            // 5. Handle Tags (Many-to-Many Relationship)
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

            // 6. Save to Database
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _productRepository.AddAsync(newProduct, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);


            // 7. Return Response
            return _productMapper.ToProductResponse(newProduct);

        }
    }
}
