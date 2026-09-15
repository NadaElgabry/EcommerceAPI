using EcommerceAPI.Application.DTOs.Recommendation;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Recommendations;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Application.Services.RecommendationService
{
    public class RecommendationService : IRecommendationService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRecommendationApiClient _recommendationApiClient;
        private readonly IProductMapper _productMapper;

        public RecommendationService(
            ICurrentUserService currentUserService,
            IRepository<User> userRepository,
            IRepository<Product> productRepository,
            IRecommendationApiClient recommendationApiClient,
            IProductMapper productMapper)
        {
            _currentUserService = currentUserService;
            _userRepository = userRepository;
            _productRepository = productRepository;
            _recommendationApiClient = recommendationApiClient;
            _productMapper = productMapper;
        }

        public async Task<List<RecommendedProductResponse>> GetRecommendationsAsync(
            int limit,
            CancellationToken cancellationToken)
        {
            if (limit < 1)
            {
                throw new BadRequestException(
                    "Recommendation limit must be at least 1.");
            }

            var user = await _userRepository.GetByAsync(
                u => u.Guid == _currentUserService.UserGuid && u.IsActive,
                cancellationToken)
                ?? throw new NotFoundException("User not found.");

            var aiResponse =
                await _recommendationApiClient.GetRecommendationsAsync(
                    user.Id,
                    limit,
                    cancellationToken);

            var orderedRecommendations = aiResponse.Items
                .OrderBy(item => item.Rank)
                .ToList();

            var productIds = orderedRecommendations
                .Select(item => item.ProductId)
                .Distinct()
                .ToList();

            var products = await _productRepository.GetAllAsync(
                predicate: product => productIds.Contains(product.Id),
                include: query => query.Include(product => product.Category),
                cancellationToken: cancellationToken);

            var productsById = products.ToDictionary(
                product => product.Id);

            var response = orderedRecommendations
                .Where(item => productsById.ContainsKey(item.ProductId))
                .Select(item => new RecommendedProductResponse
                {
                    Rank = item.Rank,
                    Score = item.Score,
                    Source = item.Source,
                    Product = _productMapper.ToProductSummaryResponse(
                        productsById[item.ProductId])
                })
                .ToList();

            return response;
        }
    }
}
