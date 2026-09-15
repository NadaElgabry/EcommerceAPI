using System.Linq.Expressions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Mappers.Mappings;
using EcommerceAPI.Application.Services.ProductReviewService;
using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;

namespace EcommerceAPI.Application.Tests.Services
{
    public class AiProductReviewServiceTests
    {
        private readonly Mock<ICurrentUserService> _currentUserService = new();
        private readonly Mock<IRepository<User>> _userRepository = new();
        private readonly Mock<IRepository<Product>> _productRepository = new();
        private readonly Mock<IRepository<ProductReview>> _reviewRepository = new();
        private readonly Mock<IRepository<Order>> _orderRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        private readonly ProductReviewMapper _mapper = new();
        private readonly ProductReviewService _sut;

        public AiProductReviewServiceTests()
        {
            _sut = new ProductReviewService(
                _currentUserService.Object,
                _userRepository.Object,
                _productRepository.Object,
                _reviewRepository.Object,
                _orderRepository.Object,
                _mapper,
                _unitOfWork.Object);
        }

        [Fact]
        public async Task GetReviewsForAiAsync_ReturnsInternalUserAndProductIds()
        {
            var reviews = new List<ProductReview>
            {
                new()
                {
                    Id = 2,
                    UserId = 25,
                    ProductId = 42,
                    Rating = 4,
                    Comment = "Good product",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = 1,
                    UserId = 10,
                    ProductId = 20,
                    Rating = 5,
                    Comment = "Excellent",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            _reviewRepository
                .Setup(repository =>
                    repository.GetAllAsync(
                        It.IsAny<Expression<Func<ProductReview, bool>>?>(),
                        It.IsAny<Func<IQueryable<ProductReview>,
                            IIncludableQueryable<ProductReview, object>>?>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(reviews);

            var result = await _sut.GetReviewsForAiAsync(
                CancellationToken.None);

            Assert.Equal(2, result.Count);

            Assert.Equal(1, result[0].ReviewId);
            Assert.Equal(10, result[0].UserId);
            Assert.Equal(20, result[0].ProductId);
            Assert.Equal(5, result[0].Rating);
            Assert.Equal("Excellent", result[0].Comment);

            Assert.Equal(2, result[1].ReviewId);
            Assert.Equal(25, result[1].UserId);
            Assert.Equal(42, result[1].ProductId);
            Assert.Equal(4, result[1].Rating);
            Assert.Equal("Good product", result[1].Comment);
        }
    }
}
