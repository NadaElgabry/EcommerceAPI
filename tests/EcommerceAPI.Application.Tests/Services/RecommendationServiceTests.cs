using System.Linq.Expressions;
using EcommerceAPI.Application.DTOs.Product;
using EcommerceAPI.Application.DTOs.Recommendation;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Recommendations;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Application.Services.RecommendationService;
using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;

namespace EcommerceAPI.Application.Tests.Services
{
    public class RecommendationServiceTests
    {
        private readonly Mock<ICurrentUserService> _currentUserService = new();
        private readonly Mock<IRepository<User>> _userRepository = new();
        private readonly Mock<IRepository<Product>> _productRepository = new();
        private readonly Mock<IRecommendationApiClient> _recommendationApiClient = new();
        private readonly Mock<IProductMapper> _productMapper = new();

        private readonly RecommendationService _sut;

        public RecommendationServiceTests()
        {
            _sut = new RecommendationService(
                _currentUserService.Object,
                _userRepository.Object,
                _productRepository.Object,
                _recommendationApiClient.Object,
                _productMapper.Object);
        }

        [Fact]
        public async Task GetRecommendationsAsync_WithValidUser_ReturnsProductsInAiRankOrder()
        {
            var userGuid = Guid.NewGuid();

            var user = new User
            {
                Id = 25,
                Guid = userGuid,
                IsActive = true
            };

            _currentUserService
                .Setup(service => service.UserGuid)
                .Returns(userGuid);

            _userRepository
                .Setup(repository =>
                    repository.GetByAsync(
                        It.IsAny<Expression<Func<User, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var aiResponse = new AiRecommendationResponse
            {
                Items = new List<AiRecommendationItem>
                {
                    new()
                    {
                        ProductId = 91,
                        Rank = 1,
                        Score = 0.95,
                        Source = "personalized"
                    },
                    new()
                    {
                        ProductId = 42,
                        Rank = 2,
                        Score = 0.87,
                        Source = "personalized"
                    },
                    new()
                    {
                        ProductId = 18,
                        Rank = 3,
                        Score = 0.79,
                        Source = "preferred_category"
                    }
                }
            };

            _recommendationApiClient
                .Setup(client =>
                    client.GetRecommendationsAsync(
                        25,
                        3,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(aiResponse);

            var product18 = new Product
            {
                Id = 18,
                Name = "Product 18",
                Slug = "product-18"
            };

            var product91 = new Product
            {
                Id = 91,
                Name = "Product 91",
                Slug = "product-91"
            };

            var product42 = new Product
            {
                Id = 42,
                Name = "Product 42",
                Slug = "product-42"
            };

            _productRepository
                .Setup(repository =>
                    repository.GetAllAsync(
                        It.IsAny<Expression<Func<Product, bool>>?>(),
                        It.IsAny<Func<IQueryable<Product>,
                            IIncludableQueryable<Product, object>>?>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product>
                {
                    product18,
                    product91,
                    product42
                });

            _productMapper
                .Setup(mapper => mapper.ToProductSummaryResponse(product91))
                .Returns(new ProductSummaryResponse
                {
                    Name = "Product 91",
                    Slug = "product-91",
                    CategorySlug = "test"
                });

            _productMapper
                .Setup(mapper => mapper.ToProductSummaryResponse(product42))
                .Returns(new ProductSummaryResponse
                {
                    Name = "Product 42",
                    Slug = "product-42",
                    CategorySlug = "test"
                });

            _productMapper
                .Setup(mapper => mapper.ToProductSummaryResponse(product18))
                .Returns(new ProductSummaryResponse
                {
                    Name = "Product 18",
                    Slug = "product-18",
                    CategorySlug = "test"
                });

            var result = await _sut.GetRecommendationsAsync(
                3,
                CancellationToken.None);

            Assert.Equal(3, result.Count);

            Assert.Equal(1, result[0].Rank);
            Assert.Equal("Product 91", result[0].Product.Name);

            Assert.Equal(2, result[1].Rank);
            Assert.Equal("Product 42", result[1].Product.Name);

            Assert.Equal(3, result[2].Rank);
            Assert.Equal("Product 18", result[2].Product.Name);

            _recommendationApiClient.Verify(
                client =>
                    client.GetRecommendationsAsync(
                        25,
                        3,
                        It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetRecommendationsAsync_WhenLimitIsLessThanOne_ThrowsBadRequestException()
        {
            await Assert.ThrowsAsync<BadRequestException>(
                () => _sut.GetRecommendationsAsync(
                    0,
                    CancellationToken.None));

            _recommendationApiClient.Verify(
                client =>
                    client.GetRecommendationsAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetRecommendationsAsync_WhenUserDoesNotExist_ThrowsNotFoundException()
        {
            var userGuid = Guid.NewGuid();

            _currentUserService
                .Setup(service => service.UserGuid)
                .Returns(userGuid);

            _userRepository
                .Setup(repository =>
                    repository.GetByAsync(
                        It.IsAny<Expression<Func<User, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.GetRecommendationsAsync(
                    10,
                    CancellationToken.None));

            _recommendationApiClient.Verify(
                client =>
                    client.GetRecommendationsAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
