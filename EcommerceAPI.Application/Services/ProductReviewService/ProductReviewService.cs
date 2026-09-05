using EcommerceAPI.Application.DTOs.ProductReview;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Application.Services.ProductReviewService
{
    public class ProductReviewService : IProductReviewService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductReview> _reviewRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IProductReviewMapper _reviewMapper;
        private readonly IUnitOfWork _unitOfWork;

        public ProductReviewService(
            ICurrentUserService currentUserService,
            IRepository<User> userRepository,
            IRepository<Product> productRepository,
            IRepository<ProductReview> reviewRepository,
            IRepository<Order> orderRepository,
            IProductReviewMapper reviewMapper,
            IUnitOfWork unitOfWork)
        {
            _currentUserService = currentUserService;
            _userRepository = userRepository;
            _productRepository = productRepository;
            _reviewRepository = reviewRepository;
            _orderRepository = orderRepository;
            _reviewMapper = reviewMapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ProductReviewResponse> CreateReviewAsync(
            string productSlug,
            CreateProductReviewRequest request,
            CancellationToken cancellationToken)
        {
            var user = await GetCurrentUserAsync(cancellationToken);

            var product = await GetProductAsync(
                productSlug,
                cancellationToken);

            var hasPurchased = await _orderRepository.ExistByAsync(
                order =>
                    order.UserId == user.Id &&
                    order.Status == OrderStatus.Delivered &&
                    order.Items.Any(item =>
                        item.ProductId == product.Id),
                cancellationToken);

            if (!hasPurchased)
            {
                throw new ForbiddenException(
                    "You can only review products you have purchased and received.");
            }

            var alreadyReviewed =
                await _reviewRepository.ExistByAsync(
                    review =>
                        review.UserId == user.Id &&
                        review.ProductId == product.Id,
                    cancellationToken);

            if (alreadyReviewed)
            {
                throw new ConflictException(
                    "You have already reviewed this product.");
            }

            var review = _reviewMapper.ToEntity(
                request,
                user.Id,
                product.Id);

            review.User = user;
            review.Product = product;

            await _reviewRepository.AddAsync(
                review,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return _reviewMapper.ToResponse(review);
        }

        public async Task<List<ProductReviewResponse>> GetProductReviewsAsync(
            string productSlug,
            CancellationToken cancellationToken)
        {
            var product = await GetProductAsync(
                productSlug,
                cancellationToken);

            var reviews = await _reviewRepository.GetAllAsync(
                predicate: review =>
                    review.ProductId == product.Id,
                include: query =>
                    query.Include(review => review.User),
                cancellationToken: cancellationToken);

            return reviews
                .OrderByDescending(review => review.CreatedAt)
                .Select(review =>
                    _reviewMapper.ToResponse(review))
                .ToList();
        }

        public async Task<List<AiProductReviewResponse>> GetReviewsForAiAsync(
            CancellationToken cancellationToken)
        {
            var reviews = await _reviewRepository.GetAllAsync(
                cancellationToken: cancellationToken);

            return reviews
                .OrderBy(review => review.Id)
                .Select(review => new AiProductReviewResponse
                {
                    ReviewId = review.Id,
                    UserId = review.UserId,
                    ProductId = review.ProductId,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt,
                    UpdatedAt = review.UpdatedAt
                })
                .ToList();
        }

        public async Task<ProductReviewResponse> UpdateReviewAsync(
            string productSlug,
            int reviewId,
            UpdateProductReviewRequest request,
            CancellationToken cancellationToken)
        {
            var user = await GetCurrentUserAsync(cancellationToken);

            var product = await GetProductAsync(
                productSlug,
                cancellationToken);

            var review = await _reviewRepository.GetByAsync(
                predicate: review =>
                    review.Id == reviewId &&
                    review.ProductId == product.Id,
                include: query =>
                    query.Include(review => review.User),
                cancellationToken: cancellationToken)
                ?? throw new NotFoundException(
                    "Review not found.");

            if (review.UserId != user.Id)
            {
                throw new ForbiddenException(
                    "You can only update your own review.");
            }

            _reviewMapper.UpdateFromRequest(
                review,
                request);

            _reviewRepository.Update(review);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return _reviewMapper.ToResponse(review);
        }

        public async Task DeleteReviewAsync(
            string productSlug,
            int reviewId,
            CancellationToken cancellationToken)
        {
            var user = await GetCurrentUserAsync(cancellationToken);

            var product = await GetProductAsync(
                productSlug,
                cancellationToken);

            var review = await _reviewRepository.GetByAsync(
                review =>
                    review.Id == reviewId &&
                    review.ProductId == product.Id,
                cancellationToken)
                ?? throw new NotFoundException(
                    "Review not found.");

            if (review.UserId != user.Id)
            {
                throw new ForbiddenException(
                    "You can only delete your own review.");
            }

            _reviewRepository.Delete(review);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }

        private async Task<User> GetCurrentUserAsync(
            CancellationToken cancellationToken)
        {
            return await _userRepository.GetByAsync(
                user =>
                    user.Guid == _currentUserService.UserGuid &&
                    user.IsActive,
                cancellationToken)
                ?? throw new NotFoundException(
                    "User not found.");
        }

        private async Task<Product> GetProductAsync(
            string productSlug,
            CancellationToken cancellationToken)
        {
            return await _productRepository.GetByAsync(
                product =>
                    product.Slug == productSlug,
                cancellationToken)
                ?? throw new NotFoundException(
                    $"Product '{productSlug}' not found.");
        }
    }
}
