using System.Linq.Expressions;
using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Category;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Image;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Interfaces.Slug;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Application.Services.CategoryService;
using Moq;
using Xunit;
using DomainCategory = EcommerceAPI.Domain.Entities.Category;

namespace EcommerceAPI.Application.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<IRepository<DomainCategory>> _categoryRepository = new();
        private readonly Mock<ICategoryMapper> _categoryMapper = new();
        private readonly Mock<IImageService> _imageService = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<ISlugGenerator> _slugGenerator = new();

        private readonly CategoryService _sut;

        public CategoryServiceTests()
        {
            _sut = new CategoryService(
                _categoryRepository.Object,
                _categoryMapper.Object,
                _imageService.Object,
                _unitOfWork.Object,
                _slugGenerator.Object
            );
        }

        [Fact]
        public async Task GetCategoriesAsync_WhenMoreCategoriesExist_ReturnsPageWithNextCursor()
        {
            // Arrange
            var request = new GetCategoriesRequest
            {
                Cursor = null,
                Limit = 2
            };

            var firstCategory = new DomainCategory
            {
                Id = 1,
                Name = "Electronics",
                Slug = "electronics",
                ImageUrl = "uploads/categories/electronics.jpg",
                CreatedAt = DateTime.UtcNow
            };

            var secondCategory = new DomainCategory
            {
                Id = 2,
                Name = "Groceries",
                Slug = "groceries",
                ImageUrl = "uploads/categories/groceries.jpg",
                CreatedAt = DateTime.UtcNow
            };

            var thirdCategory = new DomainCategory
            {
                Id = 3,
                Name = "Clothing",
                Slug = "clothing",
                ImageUrl = "uploads/categories/clothing.jpg",
                CreatedAt = DateTime.UtcNow
            };

            var categories = new List<DomainCategory>
            {
                firstCategory,
                secondCategory,
                thirdCategory
            };

            var firstResponse = new CategoryResponse
            {
                Name = firstCategory.Name,
                Slug = firstCategory.Slug,
                ImageUrl = firstCategory.ImageUrl,
                CreatedAt = firstCategory.CreatedAt
            };

            var secondResponse = new CategoryResponse
            {
                Name = secondCategory.Name,
                Slug = secondCategory.Slug,
                ImageUrl = secondCategory.ImageUrl,
                CreatedAt = secondCategory.CreatedAt
            };

            _categoryRepository
                .Setup(repository =>
                    repository.GetPagedAsync(
                        It.IsAny<Expression<Func<DomainCategory, bool>>>(),
                        It.IsAny<Expression<Func<DomainCategory, int>>>(),
                        3,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            _categoryMapper
                .Setup(mapper =>
                    mapper.toCategoryResponse(firstCategory))
                .Returns(firstResponse);

            _categoryMapper
                .Setup(mapper =>
                    mapper.toCategoryResponse(secondCategory))
                .Returns(secondResponse);

            // Act
            var result = await _sut.GetCategoriesAsync(
                request,
                CancellationToken.None
            );

            // Assert
            Assert.NotNull(result);

            Assert.Equal(
                2,
                result.Data.Count
            );

            Assert.Equal(
                "Electronics",
                result.Data[0].Name
            );

            Assert.Equal(
                "Groceries",
                result.Data[1].Name
            );

            Assert.True(
                result.Pagination.HasNext
            );

            Assert.Equal(
                2,
                result.Pagination.PageSize
            );

            Assert.Equal(
                CursorHelper.Encode(secondCategory.Id),
                result.Pagination.NextCursor
            );

            _categoryRepository.Verify(
                repository =>
                    repository.GetPagedAsync(
                        It.IsAny<Expression<Func<DomainCategory, bool>>>(),
                        It.IsAny<Expression<Func<DomainCategory, int>>>(),
                        3,
                        It.IsAny<CancellationToken>()),
                Times.Once
            );

            _categoryMapper.Verify(
                mapper =>
                    mapper.toCategoryResponse(firstCategory),
                Times.Once
            );

            _categoryMapper.Verify(
                mapper =>
                    mapper.toCategoryResponse(secondCategory),
                Times.Once
            );

            _categoryMapper.Verify(
                mapper =>
                    mapper.toCategoryResponse(thirdCategory),
                Times.Never
            );
        }

        [Fact]
        public async Task GetCategoriesAsync_WhenNoMoreCategoriesExist_ReturnsPageWithoutNextCursor()
        {
            // Arrange
            var request = new GetCategoriesRequest
            {
                Cursor = null,
                Limit = 20
            };

            var firstCategory = new DomainCategory
            {
                Id = 1,
                Name = "Electronics",
                Slug = "electronics",
                ImageUrl = "uploads/categories/electronics.jpg",
                CreatedAt = DateTime.UtcNow
            };

            var firstResponse = new CategoryResponse
            {
                Name = firstCategory.Name,
                Slug = firstCategory.Slug,
                ImageUrl = firstCategory.ImageUrl,
                CreatedAt = firstCategory.CreatedAt
            };

            var categories = new List<DomainCategory>
            {
                firstCategory
            };

            _categoryRepository
                .Setup(repository =>
                    repository.GetPagedAsync(
                        It.IsAny<Expression<Func<DomainCategory, bool>>>(),
                        It.IsAny<Expression<Func<DomainCategory, int>>>(),
                        21,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            _categoryMapper
                .Setup(mapper =>
                    mapper.toCategoryResponse(firstCategory))
                .Returns(firstResponse);

            // Act
            var result = await _sut.GetCategoriesAsync(
                request,
                CancellationToken.None
            );

            // Assert
            Assert.Single(result.Data);

            Assert.False(
                result.Pagination.HasNext
            );

            Assert.Null(
                result.Pagination.NextCursor
            );

            Assert.Equal(
                1,
                result.Pagination.PageSize
            );
        }

        [Fact]
        public async Task GetCategoriesAsync_WhenCursorProvided_UsesCursorForNextPage()
        {
            // Arrange
            var cursor = CursorHelper.Encode(2);

            var request = new GetCategoriesRequest
            {
                Cursor = cursor,
                Limit = 2
            };

            var thirdCategory = new DomainCategory
            {
                Id = 3,
                Name = "Clothing",
                Slug = "clothing",
                ImageUrl = "uploads/categories/clothing.jpg",
                CreatedAt = DateTime.UtcNow
            };

            var fourthCategory = new DomainCategory
            {
                Id = 4,
                Name = "Books",
                Slug = "books",
                ImageUrl = "uploads/categories/books.jpg",
                CreatedAt = DateTime.UtcNow
            };

            var categories = new List<DomainCategory>
            {
                thirdCategory,
                fourthCategory
            };

            var thirdResponse = new CategoryResponse
            {
                Name = thirdCategory.Name,
                Slug = thirdCategory.Slug,
                ImageUrl = thirdCategory.ImageUrl,
                CreatedAt = thirdCategory.CreatedAt
            };

            var fourthResponse = new CategoryResponse
            {
                Name = fourthCategory.Name,
                Slug = fourthCategory.Slug,
                ImageUrl = fourthCategory.ImageUrl,
                CreatedAt = fourthCategory.CreatedAt
            };

            _categoryRepository
                .Setup(repository =>
                    repository.GetPagedAsync(
                        It.IsAny<Expression<Func<DomainCategory, bool>>>(),
                        It.IsAny<Expression<Func<DomainCategory, int>>>(),
                        3,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            _categoryMapper
                .Setup(mapper =>
                    mapper.toCategoryResponse(thirdCategory))
                .Returns(thirdResponse);

            _categoryMapper
                .Setup(mapper =>
                    mapper.toCategoryResponse(fourthCategory))
                .Returns(fourthResponse);

            // Act
            var result = await _sut.GetCategoriesAsync(
                request,
                CancellationToken.None
            );

            // Assert
            Assert.Equal(
                2,
                result.Data.Count
            );

            Assert.Equal(
                "Clothing",
                result.Data[0].Name
            );

            Assert.Equal(
                "Books",
                result.Data[1].Name
            );

            Assert.False(
                result.Pagination.HasNext
            );

            Assert.Null(
                result.Pagination.NextCursor
            );
        }

        [Fact]
        public async Task GetCategoriesAsync_WhenNoCategoriesExist_ReturnsEmptyPage()
        {
            // Arrange
            var request = new GetCategoriesRequest
            {
                Cursor = null,
                Limit = 20
            };

            _categoryRepository
                .Setup(repository =>
                    repository.GetPagedAsync(
                        It.IsAny<Expression<Func<DomainCategory, bool>>>(),
                        It.IsAny<Expression<Func<DomainCategory, int>>>(),
                        21,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new List<DomainCategory>()
                );

            // Act
            var result = await _sut.GetCategoriesAsync(
                request,
                CancellationToken.None
            );

            // Assert
            Assert.NotNull(result);

            Assert.Empty(
                result.Data
            );

            Assert.False(
                result.Pagination.HasNext
            );

            Assert.Null(
                result.Pagination.NextCursor
            );

            Assert.Equal(
                0,
                result.Pagination.PageSize
            );

            _categoryMapper.Verify(
                mapper =>
                    mapper.toCategoryResponse(
                        It.IsAny<DomainCategory>()),
                Times.Never
            );
        }
    }
}