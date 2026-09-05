using System.Linq.Expressions;
using EcommerceAPI.Application.DTOs.ProductReview;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Mappers.Mappings;
using EcommerceAPI.Application.Services.ProductReviewService;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;

namespace EcommerceAPI.Application.Tests.Services
{
    public class ProductReviewServiceTests
    {
        private readonly Mock<ICurrentUserService> _currentUserService = new();
        private readonly Mock<IRepository<User>> _userRepository = new();
        private readonly Mock<IRepository<Product>> _productRepository = new();
        private readonly Mock<IRepository<ProductReview>> _reviewRepository = new();
        private readonly Mock<IRepository<Order>> _orderRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        private readonly ProductReviewMapper _mapper = new();
        private readonly ProductReviewService _sut;

        public ProductReviewServiceTests()
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
        public async Task CreateReviewAsync_WithDeliveredPurchase_CreatesReview()
        {
            var user = CreateUser(1);
            var product = CreateProduct(10);

            SetupCurrentUser(user);
            SetupProduct(product);

            var deliveredOrder = new Order
            {
                Id = 1,
                UserId = user.Id,
                IdempotencyKey = "test-order-1",
                Address = "Test Address",
                Status = OrderStatus.Delivered,
                Items = new List<OrderItem>
                {
                    new()
                    {
                        ProductId = product.Id,
                        Quantity = 1,
                        UnitPrice = 100m
                    }
                }
            };

            _orderRepository
                .Setup(repository =>
                    repository.ExistByAsync(
                        It.IsAny<Expression<Func<Order, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync((
                    Expression<Func<Order, bool>> predicate,
                    CancellationToken _) =>
                    predicate.Compile()(deliveredOrder));

            _reviewRepository
                .Setup(repository =>
                    repository.ExistByAsync(
                        It.IsAny<Expression<Func<ProductReview, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _unitOfWork
                .Setup(unitOfWork =>
                    unitOfWork.SaveChangesAsync(
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var request = new CreateProductReviewRequest
            {
                Rating = 5,
                Comment = "Great product"
            };

            var result = await _sut.CreateReviewAsync(
                product.Slug,
                request,
                CancellationToken.None);

            Assert.Equal(5, result.Rating);
            Assert.Equal("Great product", result.Comment);
            Assert.Equal(user.Guid, result.UserGuid);

            _reviewRepository.Verify(
                repository =>
                    repository.AddAsync(
                        It.Is<ProductReview>(review =>
                            review.UserId == user.Id &&
                            review.ProductId == product.Id &&
                            review.Rating == 5),
                        It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWork.Verify(
                unitOfWork =>
                    unitOfWork.SaveChangesAsync(
                        It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateReviewAsync_WhenOrderIsNotDelivered_ThrowsForbiddenException()
        {
            var user = CreateUser(1);
            var product = CreateProduct(10);

            SetupCurrentUser(user);
            SetupProduct(product);

            var placedOrder = new Order
            {
                Id = 1,
                UserId = user.Id,
                IdempotencyKey = "test-order-1",
                Address = "Test Address",
                Status = OrderStatus.Placed,
                Items = new List<OrderItem>
                {
                    new()
                    {
                        ProductId = product.Id,
                        Quantity = 1,
                        UnitPrice = 100m
                    }
                }
            };

            _orderRepository
                .Setup(repository =>
                    repository.ExistByAsync(
                        It.IsAny<Expression<Func<Order, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync((
                    Expression<Func<Order, bool>> predicate,
                    CancellationToken _) =>
                    predicate.Compile()(placedOrder));

            var request = new CreateProductReviewRequest
            {
                Rating = 4
            };

            await Assert.ThrowsAsync<ForbiddenException>(
                () => _sut.CreateReviewAsync(
                    product.Slug,
                    request,
                    CancellationToken.None));

            _reviewRepository.Verify(
                repository =>
                    repository.AddAsync(
                        It.IsAny<ProductReview>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateReviewAsync_WhenAlreadyReviewed_ThrowsConflictException()
        {
            var user = CreateUser(1);
            var product = CreateProduct(10);

            SetupCurrentUser(user);
            SetupProduct(product);

            _orderRepository
                .Setup(repository =>
                    repository.ExistByAsync(
                        It.IsAny<Expression<Func<Order, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _reviewRepository
                .Setup(repository =>
                    repository.ExistByAsync(
                        It.IsAny<Expression<Func<ProductReview, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var request = new CreateProductReviewRequest
            {
                Rating = 5
            };

            await Assert.ThrowsAsync<ConflictException>(
                () => _sut.CreateReviewAsync(
                    product.Slug,
                    request,
                    CancellationToken.None));

            _reviewRepository.Verify(
                repository =>
                    repository.AddAsync(
                        It.IsAny<ProductReview>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateReviewAsync_WhenReviewBelongsToUser_UpdatesReview()
        {
            var user = CreateUser(1);
            var product = CreateProduct(10);

            var review = new ProductReview
            {
                Id = 20,
                UserId = user.Id,
                User = user,
                ProductId = product.Id,
                Product = product,
                Rating = 3,
                Comment = "Old comment"
            };

            SetupCurrentUser(user);
            SetupProduct(product);
            SetupReviewWithUser(review);

            _unitOfWork
                .Setup(unitOfWork =>
                    unitOfWork.SaveChangesAsync(
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var request = new UpdateProductReviewRequest
            {
                Rating = 5,
                Comment = "Updated comment"
            };

            var result = await _sut.UpdateReviewAsync(
                product.Slug,
                review.Id,
                request,
                CancellationToken.None);

            Assert.Equal(5, result.Rating);
            Assert.Equal("Updated comment", result.Comment);
            Assert.NotNull(result.UpdatedAt);

            _reviewRepository.Verify(
                repository => repository.Update(review),
                Times.Once);

            _unitOfWork.Verify(
                unitOfWork =>
                    unitOfWork.SaveChangesAsync(
                        It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateReviewAsync_WhenReviewBelongsToAnotherUser_ThrowsForbiddenException()
        {
            var currentUser = CreateUser(1);
            var owner = CreateUser(2);
            var product = CreateProduct(10);

            var review = new ProductReview
            {
                Id = 20,
                UserId = owner.Id,
                User = owner,
                ProductId = product.Id,
                Product = product,
                Rating = 4
            };

            SetupCurrentUser(currentUser);
            SetupProduct(product);
            SetupReviewWithUser(review);

            var request = new UpdateProductReviewRequest
            {
                Rating = 5
            };

            await Assert.ThrowsAsync<ForbiddenException>(
                () => _sut.UpdateReviewAsync(
                    product.Slug,
                    review.Id,
                    request,
                    CancellationToken.None));

            _reviewRepository.Verify(
                repository =>
                    repository.Update(
                        It.IsAny<ProductReview>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteReviewAsync_WhenReviewBelongsToUser_DeletesReview()
        {
            var user = CreateUser(1);
            var product = CreateProduct(10);

            var review = new ProductReview
            {
                Id = 20,
                UserId = user.Id,
                User = user,
                ProductId = product.Id,
                Product = product,
                Rating = 5
            };

            SetupCurrentUser(user);
            SetupProduct(product);
            SetupReview(review);

            _unitOfWork
                .Setup(unitOfWork =>
                    unitOfWork.SaveChangesAsync(
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await _sut.DeleteReviewAsync(
                product.Slug,
                review.Id,
                CancellationToken.None);

            _reviewRepository.Verify(
                repository => repository.Delete(review),
                Times.Once);

            _unitOfWork.Verify(
                unitOfWork =>
                    unitOfWork.SaveChangesAsync(
                        It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteReviewAsync_WhenReviewBelongsToAnotherUser_ThrowsForbiddenException()
        {
            var currentUser = CreateUser(1);
            var owner = CreateUser(2);
            var product = CreateProduct(10);

            var review = new ProductReview
            {
                Id = 20,
                UserId = owner.Id,
                User = owner,
                ProductId = product.Id,
                Product = product,
                Rating = 5
            };

            SetupCurrentUser(currentUser);
            SetupProduct(product);
            SetupReview(review);

            await Assert.ThrowsAsync<ForbiddenException>(
                () => _sut.DeleteReviewAsync(
                    product.Slug,
                    review.Id,
                    CancellationToken.None));

            _reviewRepository.Verify(
                repository =>
                    repository.Delete(
                        It.IsAny<ProductReview>()),
                Times.Never);
        }

        [Fact]
        public async Task GetProductReviewsAsync_ReturnsNewestReviewsFirst()
        {
            var product = CreateProduct(10);
            var firstUser = CreateUser(1);
            var secondUser = CreateUser(2);

            SetupProduct(product);

            var olderReview = new ProductReview
            {
                Id = 1,
                UserId = firstUser.Id,
                User = firstUser,
                ProductId = product.Id,
                Product = product,
                Rating = 4,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };

            var newerReview = new ProductReview
            {
                Id = 2,
                UserId = secondUser.Id,
                User = secondUser,
                ProductId = product.Id,
                Product = product,
                Rating = 5,
                CreatedAt = DateTime.UtcNow
            };

            _reviewRepository
                .Setup(repository =>
                    repository.GetAllAsync(
                        It.IsAny<Expression<Func<ProductReview, bool>>?>(),
                        It.IsAny<Func<IQueryable<ProductReview>,
                            IIncludableQueryable<ProductReview, object>>?>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductReview>
                {
                    olderReview,
                    newerReview
                });

            var result =
                await _sut.GetProductReviewsAsync(
                    product.Slug,
                    CancellationToken.None);

            Assert.Equal(2, result.Count);
            Assert.Equal(newerReview.Id, result[0].Id);
            Assert.Equal(olderReview.Id, result[1].Id);
        }

        private void SetupCurrentUser(User user)
        {
            _currentUserService
                .Setup(service => service.UserGuid)
                .Returns(user.Guid);

            _userRepository
                .Setup(repository =>
                    repository.GetByAsync(
                        It.IsAny<Expression<Func<User, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
        }

        private void SetupProduct(Product product)
        {
            _productRepository
                .Setup(repository =>
                    repository.GetByAsync(
                        It.IsAny<Expression<Func<Product, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
        }

        private void SetupReview(ProductReview review)
        {
            _reviewRepository
                .Setup(repository =>
                    repository.GetByAsync(
                        It.IsAny<Expression<Func<ProductReview, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(review);
        }

        private void SetupReviewWithUser(ProductReview review)
        {
            _reviewRepository
                .Setup(repository =>
                    repository.GetByAsync(
                        It.IsAny<Expression<Func<ProductReview, bool>>>(),
                        It.IsAny<Func<IQueryable<ProductReview>,
                            IIncludableQueryable<ProductReview, object>>?>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(review);
        }

        private static User CreateUser(int id)
        {
            return new User
            {
                Id = id,
                Guid = Guid.NewGuid(),
                FirstName = $"User{id}",
                LastName = "Test",
                IsActive = true
            };
        }

        private static Product CreateProduct(int id)
        {
            return new Product
            {
                Id = id,
                Slug = $"product-{id}",
                Name = $"Product {id}",
                Description = "Test product",
                Price = 100m,
                StockQuantity = 10,
                ProductImage = "image.jpg",
                AltText = "Test product",
                CreationDate = DateTime.UtcNow
            };
        }
    }
}



