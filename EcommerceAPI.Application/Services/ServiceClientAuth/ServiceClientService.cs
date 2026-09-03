using EcommerceAPI.Application.DTOs.ServiceAuth;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces;
using EcommerceAPI.Application.Interfaces.Auth;
using EcommerceAPI.Application.Interfaces.IServices;
using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Services.ServiceClientAuth
{
    public class ServiceClientService : IServiceClientService
    {
        private readonly IRepository<ServiceClient> _serviceClientRepository;
        private readonly IServiceClientMapper _serviceClientMapper;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;

        public ServiceClientService(
            IRepository<ServiceClient> serviceClientRepository,
            IServiceClientMapper serviceClientMapper,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IUnitOfWork unitOfWork)
        {
            _serviceClientRepository = serviceClientRepository;
            _serviceClientMapper = serviceClientMapper;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc />
        public async Task<CreateServiceClientResponse> CreateAsync(
            CreateServiceClientRequest request, CancellationToken cancellationToken)
        {
            var clientId = $"svc-{Guid.NewGuid():N}"[..16];
            var rawSecret = _tokenService.GenerateHighEntropyToken();
            var secretHash = _passwordHasher.Hash(rawSecret);

            var serviceClient = _serviceClientMapper.ToEntity(request, clientId, secretHash);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _serviceClientRepository.AddAsync(serviceClient, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);

            return _serviceClientMapper.ToCreateResponse(serviceClient, rawSecret);
        }

        /// <inheritdoc />
        public async Task<ServiceTokenResponse> IssueTokenAsync(
            ServiceTokenRequest request, CancellationToken cancellationToken)
        {
            var client = await _serviceClientRepository.GetByAsync(
                c => c.ClientId == request.ClientId && c.IsActive,
                cancellationToken)
                ?? throw new UnauthorizedException("Invalid client credentials.");

            if (!_passwordHasher.Verify(request.ClientSecret, client.ClientSecretHash))
                throw new UnauthorizedException("Invalid client credentials.");

            var token = _tokenService.GenerateServiceToken(client.ClientId, client.Scopes);

            return new ServiceTokenResponse
            {
                AccessToken = token.Token,
                ExpiresAtUtc = token.ExpiresAtUtc
            };
        }

        /// <inheritdoc />
        public async Task RevokeAsync(string clientId, CancellationToken cancellationToken)
        {
            var client = await _serviceClientRepository.GetByAsync(
                c => c.ClientId == clientId, cancellationToken)
                ?? throw new NotFoundException("Service client not found.");

            client.IsActive = false;
            client.RevokedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}