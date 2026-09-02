using EcommerceAPI.Application.DTOs.Product;
using EcommerceAPI.Application.DTOs.Rag;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.ExternalServices.Rag;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EcommerceAPI.Application.Services.RagService
{
    public class RagService : IRagService
    {
        private readonly IRagClient _ragClient;
        private readonly IRepository<Product> _productRepository;
        private readonly IProductMapper _productMapper;
        private readonly ILogger<RagService> _logger;
        private readonly IRepository<User> _userRepository;
        private readonly ICurrentUserService _currentUserService;

        public RagService(
            IRagClient ragClient,
            IRepository<Product> productRepository,
            IProductMapper productMapper,
            IRepository<User> userRepository,
            ILogger<RagService> logger,
            ICurrentUserService currentUserService)
        {
            _ragClient = ragClient;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _productMapper = productMapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<AnswerResponse> AskAsync(string question, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByAsync(u => u.Guid == _currentUserService.UserGuid, cancellationToken)
                ?? throw new NotFoundException("User not found");
            var request = new QuestionRequest { Question = question, UserId = user.Id.ToString() };
            return await _ragClient.AskAsync(request, cancellationToken);
        }

        public async Task<TerminationResult> TerminateAsync(CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByAsync(u => u.Guid == _currentUserService.UserGuid, cancellationToken)
                ?? throw new NotFoundException("User not found");
            var raw = await _ragClient.TerminateAsync(user.Id.ToString(), cancellationToken);

            var products = new List<ProductSummaryResponse>();
            foreach (var id in raw.SuggestedProducts.Distinct())
            {
                var product = await _productRepository.GetByAsync(p => p.Id == id, cancellationToken);
                if (product != null)
                {
                    products.Add(_productMapper.ToProductSummaryResponse(product));
                }
                else
                {
                    _logger.LogWarning("RAG service suggested product {ProductId} which no longer exists.", id);
                }
            }

            return new TerminationResult
            {
                UserId = raw.UserId,
                SuggestedProducts = products
            };
        }
    }
}