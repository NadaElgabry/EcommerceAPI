using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Category;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Image;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Interfaces.Slug;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Domain.Enums;

namespace EcommerceAPI.Application.Services.CategoryService
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly ICategoryMapper _categoryMapper;
        private readonly IImageService _imageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISlugGenerator _slugGenerator;

        public CategoryService(
            IRepository<Category> categoryRepository,
            ICategoryMapper categoryMapper,
            IImageService imageService,
            IUnitOfWork unitOfWork,
            ISlugGenerator slugGenerator)
        {
            _categoryRepository = categoryRepository;
            _categoryMapper = categoryMapper;
            _imageService = imageService;
            _unitOfWork = unitOfWork;
            _slugGenerator = slugGenerator;
        }

        public async Task<CategoryResponse> CreateCategoryAsync(
            CreateCategoryRequest request,
            CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByAsync(
                c => c.Name == request.Name,
                cancellationToken);

            if (category != null)
            {
                throw new ConflictException(
                    "Category with the same name already exists.");
            }

            var slug = _slugGenerator.GenerateSlug(request.Name);

            if (await _categoryRepository.ExistByAsync(
                c => c.Slug == slug,
                cancellationToken))
            {
                throw new ConflictException(
                    "A category with a matching slug already exists.");
            }

            var imageUrl = await _imageService.SaveFileAsync(
                request.Image,
                slug,
                ImageOwnerType.Category,
                cancellationToken);

            var newCategory = new Category
            {
                Name = request.Name,
                Slug = slug,
                ImageUrl = imageUrl
            };

            await _unitOfWork.ExecuteInTransactionAsync(
                async () =>
                {
                    await _categoryRepository.AddAsync(
                        newCategory,
                        cancellationToken);

                    await _unitOfWork.SaveChangesAsync(
                        cancellationToken);
                },
                cancellationToken);

            return _categoryMapper.toCategoryResponse(
                newCategory);
        }

        public async Task<CursorPagedResult<CategoryResponse>> GetCategoriesAsync(
            GetCategoriesRequest request,
            CancellationToken cancellationToken)
        {
            var lastCategoryId = 0;

            if (!string.IsNullOrWhiteSpace(request.Cursor))
            {
                lastCategoryId = CursorHelper.Decode<int>(
                    request.Cursor);
            }

            var categories = await _categoryRepository.GetPagedAsync(
                category => category.Id > lastCategoryId,
                category => category.Id,
                request.Limit + 1,
                cancellationToken);

            var hasNext = categories.Count > request.Limit;

            if (hasNext)
            {
                categories.RemoveAt(categories.Count - 1);
            }

            string? nextCursor = null;

            if (hasNext && categories.Count > 0)
            {
                nextCursor = CursorHelper.Encode(
                    categories[^1].Id);
            }

            var categoryResponses = categories
                .Select(category =>
                    _categoryMapper.toCategoryResponse(category))
                .ToList();

            return new CursorPagedResult<CategoryResponse>
            {
                Data = categoryResponses,

                Pagination = new CursorPageInfo
                {
                    NextCursor = nextCursor,
                    HasNext = hasNext,
                    PageSize = categoryResponses.Count
                }
            };
        }
    }
}