using EcommerceAPI.Application.DTOs.ServiceAuth;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface IServiceClientService
    {
        Task<CreateServiceClientResponse> CreateAsync(CreateServiceClientRequest request, CancellationToken cancellationToken);
        Task<ServiceTokenResponse> IssueTokenAsync(ServiceTokenRequest request, CancellationToken cancellationToken);
        Task RevokeAsync(string clientId, CancellationToken cancellationToken);
    }
}