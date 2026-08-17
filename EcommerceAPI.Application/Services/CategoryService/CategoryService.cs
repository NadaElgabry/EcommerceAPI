using EcommerceAPI.Application.DTOs.Category;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Image;
using EcommerceAPI.Application.Interfaces.Iservices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Services.CategoryService
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _categoryRepository; 
        private readonly ICategoryMapper _categoryMapper;
        private readonly IImageService _imageService;
        private readonly IUnitOfWork _unitOfWork;
        public CategoryService(IRepository<Category> categoryRepository,
            ICategoryMapper categoryMapper, IImageService imageService
            ,IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _categoryMapper = categoryMapper;
            _imageService = imageService;
            _unitOfWork = unitOfWork;
        }

        public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByAsync(c => c.Name == request.Name, cancellationToken);
            
            if(category != null)
            {
                throw new ConflictException("Category with the same name already exists.");
            }
            
            var imageurl = await _imageService.SaveFileAsync(request.Image);
            var newCategory = new Category
            {
                Name = request.Name,
                ImageUrl = imageurl,
            };

            await _categoryRepository.AddAsync(newCategory, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _categoryMapper.toCategoryResponse(newCategory);

        }
    }
}
